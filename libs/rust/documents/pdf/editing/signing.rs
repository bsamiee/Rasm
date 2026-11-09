use std::{collections::HashSet, io};

use hayro_syntax::object::dict::keys::{ACRO_FORM, ANNOTS, F, FIELDS, FT, P, RECT, ROOT, SIG_FLAGS, SUBTYPE, T, TYPE, V};
use lopdf::{IncrementalDocument, Object, ObjectId, decode_text_string, dictionary, text_string, xref::XrefType};
use pdf_oxide::{
    crypto::{self, HashAlgorithm},
    signatures::{DigestAlgorithm, PadesLevel, RevocationMaterial, SignOptions, SignatureSubFilter, SigningCredentials, sign_pdf_bytes_pades},
};
use rasn::{der, types::Oid};
use rasn_cms::{
    AlgorithmIdentifier, SignedData,
    tsp::{MessageImprint, PkiStatus, TST_INFO, TimeStampReq, TimeStampResp, TstInfo},
};
use reqwest::{Url, blocking::Client, header::CONTENT_TYPE};

use crate::{Document, PdfError, entry, resolved};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Authority {
    pub client: Client,
    pub url: Url,
}

#[derive(Debug, Clone)]
pub enum Level {
    Basic,
    Timestamp(Authority),
    LongTerm { authority: Authority, material: RevocationMaterial },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn sign(document: &Document, credentials: &SigningCredentials, options: SignOptions, level: &Level) -> Result<Document, PdfError> {
    fn fielded(document: &Document) -> Result<Vec<u8>, PdfError> {
        const SIGNATURES_EXIST_APPEND_ONLY: i64 = 3;
        const PRINT_LOCKED: i64 = 132;
        fn appended(update: &mut IncrementalDocument, holder: ObjectId, key: &[u8], items: Vec<Object>) -> Result<(), lopdf::Error> {
            let extend = |target: &mut Object| target.as_array_mut().map(|array| array.extend(items));
            update.opt_clone_object_to_new_document(holder)?;
            let listed = update.new_document.get_dictionary_mut(holder)?.as_hashmap_mut().entry(key.to_vec()).or_insert_with(|| Object::Array(Vec::new()));
            if let Object::Reference(list) = listed {
                let list = *list;
                update.opt_clone_object_to_new_document(list)?;
                extend(update.new_document.get_object_mut(list)?)
            } else {
                extend(listed)
            }
        }
        (!lopdf::Document::load_metadata_mem(document.data().as_ref())?.encrypted).ok_or(PdfError::Encrypted)?;
        let bytes = document.data().as_ref().to_vec();
        let mut prior = lopdf::Document::load_mem(&bytes)?;
        prior.reference_table.cross_reference_type = XrefType::CrossReferenceTable;
        let root = prior.trailer.get(ROOT)?.as_reference()?;
        let page = prior.page_iter().next().ok_or(PdfError::Page { index: 0, count: 0 })?;
        let catalog = prior.get_dictionary(root)?;
        let form = catalog.as_hashmap().get(ACRO_FORM).cloned();
        let roots = match form.as_ref().map(|form| resolved(&prior, form)).transpose()? {
            Some(Object::Dictionary(held)) => entry(&prior, held, FIELDS)?.map(Object::as_array).transpose()?.cloned().unwrap_or_default(),
            Some(_) | None => Vec::new(),
        };
        let taken = roots
            .iter()
            .map(|field| resolved(&prior, field).and_then(Object::as_dict).and_then(|field| entry(&prior, field, T)?.map(decode_text_string).transpose()))
            .try_collect::<Vec<_>>()?
            .into_iter()
            .flatten()
            .collect::<HashSet<_>>();
        let mut update = IncrementalDocument::create_from(bytes, prior);
        let named = |ordinal: usize| format!("Signature{ordinal}");
        let name = (1..=taken.len()).map(named).find(|name| !taken.contains(name)).unwrap_or_else(|| named(taken.len().strict_add(1)));
        let widget = update
            .new_document
            .try_add_object(dictionary! { TYPE => "Annot", SUBTYPE => "Widget", FT => "Sig", T => text_string(&name), F => PRINT_LOCKED, RECT => vec![Object::from(0); 4], P => page })?;
        let holder = match &form {
            Some(Object::Reference(held)) => *held,
            Some(Object::Dictionary(inline)) => update.new_document.try_add_object(inline.clone())?,
            Some(_) | None => update.new_document.try_add_object(dictionary! { FIELDS => Vec::<Object>::new() })?,
        };
        if !matches!(form, Some(Object::Reference(_))) {
            update.opt_clone_object_to_new_document(root)?;
            update.new_document.get_dictionary_mut(root)?.set(ACRO_FORM, holder);
        }
        appended(&mut update, holder, FIELDS, vec![Object::Reference(widget)])?;
        appended(&mut update, page, ANNOTS, vec![Object::Reference(widget)])?;
        update.opt_clone_object_to_new_document(holder)?;
        update.new_document.get_dictionary_mut(holder)?.set(SIG_FLAGS, SIGNATURES_EXIST_APPEND_ONLY);
        let size = update.new_document.max_id.checked_add(1).ok_or(lopdf::Error::ObjectIdOverflow)?;
        update.new_document.get_dictionary_mut(widget)?.set(V, Object::Reference((size, 0)));
        let mut fielded = Vec::new();
        update.save_to(&mut fielded).map_err(lopdf::Error::from)?;
        Ok(fielded)
    }
    fn token(authority: &Authority, algorithm: DigestAlgorithm, message: &[u8]) -> Result<Vec<u8>, PdfError> {
        let (hash, oid) = match algorithm {
            DigestAlgorithm::Sha1 => (HashAlgorithm::Sha1, Oid::ISO_IDENTIFIED_ORGANISATION_OIW_SECSIG_ALGORITHM_SHA1),
            DigestAlgorithm::Sha256 => (HashAlgorithm::Sha256, Oid::JOINT_ISO_ITU_T_COUNTRY_US_ORGANIZATION_GOV_CSOR_NIST_ALGORITHMS_HASH_SHA256),
            DigestAlgorithm::Sha384 => (HashAlgorithm::Sha384, Oid::JOINT_ISO_ITU_T_COUNTRY_US_ORGANIZATION_GOV_CSOR_NIST_ALGORITHMS_HASH_SHA384),
            DigestAlgorithm::Sha512 => (HashAlgorithm::Sha512, Oid::JOINT_ISO_ITU_T_COUNTRY_US_ORGANIZATION_GOV_CSOR_NIST_ALGORITHMS_HASH_SHA512),
        };
        let request = TimeStampReq {
            version: 1,
            message_imprint: MessageImprint {
                hash_algorithm: AlgorithmIdentifier { algorithm: oid.into(), parameters: None },
                hashed_message: {
                    let mut hasher = crypto::active().hasher(hash).map_err(|source| pdf_oxide::Error::from(io::Error::other(source)))?;
                    hasher.update(message);
                    hasher.finalize().into()
                },
            },
            req_policy: None,
            nonce: None,
            cert_req: true,
            extensions: None,
        };
        let response = authority.client.post(authority.url.clone()).header(CONTENT_TYPE, "application/timestamp-query").body(der::encode(&request)?).send()?.error_for_status()?.bytes()?;
        let response: TimeStampResp = der::decode(&response)?;
        let stamp = match response.status.status {
            PkiStatus::Granted | PkiStatus::GrantedWithMods => response.time_stamp_token.ok_or(PdfError::Timestamp(None)),
            status @ (PkiStatus::Rejection | PkiStatus::Waiting | PkiStatus::RevocationWarning | PkiStatus::RevocationNotification) => Err(PdfError::Timestamp(Some(status))),
        }?;
        (stamp.content_type == rasn_cms::CONTENT_SIGNED_DATA).ok_or(PdfError::Timestamp(None))?;
        let signed: SignedData = der::decode(stamp.content.as_bytes())?;
        (signed.encap_content_info.content_type == TST_INFO).ok_or(PdfError::Timestamp(None))?;
        let info: TstInfo = der::decode(signed.encap_content_info.content.as_deref().ok_or(PdfError::Timestamp(None))?)?;
        (info.message_imprint.hash_algorithm.algorithm == request.message_imprint.hash_algorithm.algorithm && info.message_imprint.hashed_message == request.message_imprint.hashed_message)
            .ok_or(PdfError::Imprint)?;
        Ok(der::encode(&stamp)?)
    }
    let unvalidated = RevocationMaterial::default();
    let (pades, authority, material) = match level {
        Level::Basic => (PadesLevel::BB, None, &unvalidated),
        Level::Timestamp(authority) => (PadesLevel::BT, Some(authority), &unvalidated),
        Level::LongTerm { authority, material } => (PadesLevel::BLt, Some(authority), material),
    };
    let algorithm = options.digest_algorithm;
    let timestamper = authority.map(|authority| move |message: &[u8]| token(authority, algorithm, message).map_err(|error| pdf_oxide::Error::from(io::Error::other(error))));
    let signed = sign_pdf_bytes_pades(
        &fielded(document)?,
        credentials,
        SignOptions { sub_filter: SignatureSubFilter::CadesDetached, ..options },
        pades,
        timestamper.as_ref().map(|stamp| stamp as &dyn Fn(&[u8]) -> pdf_oxide::Result<Vec<u8>>),
        material,
    )
    .map_err(|engine| match engine {
        pdf_oxide::Error::Io(carried) => carried.downcast::<PdfError>().unwrap_or_else(|other| PdfError::Engine(other.into())),
        other => PdfError::Engine(other),
    })?;
    Document::new(signed, None)
}
