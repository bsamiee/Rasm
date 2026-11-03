use core::{iter, num::TryFromIntError};
use std::{collections::HashSet, io};

use lopdf::{IncrementalDocument, Object, ObjectId, decode_text_string, dictionary, text_string};
use pdf_oxide::{
    crypto::{self, HashAlgorithm},
    signatures::{DigestAlgorithm, PadesLevel, RevocationMaterial, SignOptions, SignatureSubFilter, SigningCredentials, Timestamp, sign_pdf_bytes_pades},
};
use reqwest::{Url, blocking::Client, header::CONTENT_TYPE};

use crate::{Document, PdfError, entry, resolved};

// --- [CONSTANTS] -----------------------------------------------------------------------

const LONG_FORM: u8 = 0x80;

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
    Archival { authority: Authority, material: RevocationMaterial },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn sign(document: &Document, credentials: &SigningCredentials, options: SignOptions, level: &Level) -> Result<Document, PdfError> {
    fn fielded(document: &Document, level: PadesLevel, material: &RevocationMaterial) -> Result<Vec<u8>, PdfError> {
        const ACROFORM: &str = "AcroForm";
        const FIELDS: &str = "Fields";
        const SIGNATURES_EXIST_APPEND_ONLY: i64 = 3;
        const PRINT_LOCKED: i64 = 132;
        fn appended(update: &mut IncrementalDocument, holder: ObjectId, key: &str, items: Vec<Object>) -> Result<(), lopdf::Error> {
            let holding = if update.new_document.has_object(holder) { &update.new_document } else { update.get_prev_documents() }.get_dictionary(holder)?;
            match holding.has(key.as_bytes()).then(|| holding.get(key.as_bytes()).cloned()).transpose()? {
                Some(Object::Reference(list)) => {
                    update.opt_clone_object_to_new_document(list)?;
                    update.new_document.get_object_mut(list)?.as_array_mut().map(|listed| listed.extend(items))
                }
                Some(Object::Array(listed)) => {
                    update.opt_clone_object_to_new_document(holder)?;
                    update.new_document.get_dictionary_mut(holder).map(|dictionary| dictionary.set(key, [listed, items].concat()))
                }
                Some(_) | None => {
                    update.opt_clone_object_to_new_document(holder)?;
                    update.new_document.get_dictionary_mut(holder).map(|dictionary| dictionary.set(key, items))
                }
            }
        }
        let bytes = document.data().as_ref().to_vec();
        let prior = lopdf::Document::load_mem(&bytes)?;
        let root = prior.trailer.get(b"Root")?.as_reference()?;
        let page = prior.page_iter().next().ok_or(PdfError::Page { index: 0, count: 0 })?;
        let catalog = prior.get_dictionary(root)?;
        let form = catalog.has(ACROFORM.as_bytes()).then(|| catalog.get(ACROFORM.as_bytes()).cloned()).transpose()?;
        let roots = match form.as_ref().map(|form| resolved(&prior, form)).transpose()? {
            Some(Object::Dictionary(held)) => entry(&prior, held, FIELDS)?.map(Object::as_array).transpose()?.cloned().unwrap_or_default(),
            Some(_) | None => Vec::new(),
        };
        let taken = roots
            .iter()
            .map(|field| resolved(&prior, field).and_then(Object::as_dict).and_then(|field| entry(&prior, field, "T")?.map(decode_text_string).transpose()))
            .try_collect::<Vec<_>>()?
            .into_iter()
            .flatten()
            .collect::<HashSet<_>>();
        let mut update = IncrementalDocument::create_from(bytes, prior);
        let widgets = (1_usize..)
            .map(|ordinal| format!("Signature{ordinal}"))
            .filter(|name| !taken.contains(name))
            .take(if level == PadesLevel::BLta { 2 } else { 1 })
            .map(|name| {
                update.new_document.try_add_object(
                    dictionary! { "Type" => "Annot", "Subtype" => "Widget", "FT" => "Sig", "T" => text_string(&name), "F" => PRINT_LOCKED, "Rect" => vec![Object::from(0); 4], "P" => page },
                )
            })
            .try_collect::<Vec<_>>()?;
        let listed = widgets.iter().copied().map(Object::Reference).collect::<Vec<_>>();
        let holder = match &form {
            Some(Object::Reference(held)) => *held,
            Some(Object::Dictionary(inline)) => update.new_document.try_add_object(inline.clone())?,
            Some(_) | None => update.new_document.try_add_object(dictionary! { FIELDS => Vec::<Object>::new() })?,
        };
        if !matches!(form, Some(Object::Reference(_))) {
            update.opt_clone_object_to_new_document(root)?;
            update.new_document.get_dictionary_mut(root)?.set(ACROFORM, holder);
        }
        appended(&mut update, holder, FIELDS, listed.clone())?;
        appended(&mut update, page, "Annots", listed)?;
        update.opt_clone_object_to_new_document(holder)?;
        update.new_document.get_dictionary_mut(holder)?.set("SigFlags", SIGNATURES_EXIST_APPEND_ONLY);
        let signature = update.new_document.max_id.strict_add(1);
        let items = [&material.certificates, &material.crls, &material.ocsp_responses].into_iter().map(Vec::len).fold(0_usize, usize::strict_add);
        let stamp = signature.checked_add(2).and_then(|after| after.checked_add(u32::try_from(items).unwrap_or(u32::MAX))).ok_or(lopdf::Error::ObjectIdOverflow)?;
        iter::zip(&widgets, [signature, stamp]).try_for_each(|(&widget, value)| update.new_document.get_dictionary_mut(widget).map(|field| field.set("V", Object::Reference((value, 0)))))?;
        let mut fielded = Vec::new();
        update.save_to(&mut fielded).map_err(lopdf::Error::from)?;
        Ok(fielded)
    }
    fn token(authority: &Authority, algorithm: DigestAlgorithm, message: &[u8]) -> Result<Vec<u8>, PdfError> {
        fn tagged(tag: u8, content: &[u8]) -> Vec<u8> {
            let header = match u8::try_from(content.len()) {
                Ok(short) if short < LONG_FORM => vec![tag, short],
                Ok(_) | Err(TryFromIntError { .. }) => {
                    let significant = content.len().to_be_bytes().into_iter().skip_while(|byte| *byte == 0).collect::<Vec<_>>();
                    iter::chain([tag, iter::zip(1_u8.., &significant).last().map_or(LONG_FORM, |(count, _)| LONG_FORM | count)], significant).collect()
                }
            };
            [header.as_slice(), content].concat()
        }
        fn element(der: &[u8]) -> Option<(&[u8], &[u8], &[u8])> {
            let [_, first, tail @ ..] = der else {
                return None;
            };
            let extra = if *first < LONG_FORM { 0 } else { usize::from(first & !LONG_FORM) };
            let size = if *first < LONG_FORM { usize::from(*first) } else { tail.get(..extra)?.iter().try_fold(0_usize, |size, byte| size.checked_mul(256)?.checked_add(usize::from(*byte)))? };
            let header = extra.checked_add(2)?;
            let end = header.checked_add(size)?;
            Some((der.get(..end)?, der.get(header..end)?, der.get(end..)?))
        }
        const BOOLEAN: u8 = 0x01;
        const INTEGER: u8 = 0x02;
        const OCTET_STRING: u8 = 0x04;
        const OBJECT_IDENTIFIER: u8 = 0x06;
        const SEQUENCE: u8 = 0x30;
        let imprint = {
            let mut hasher = crypto::active()
                .hasher(match algorithm {
                    DigestAlgorithm::Sha1 => HashAlgorithm::Sha1,
                    DigestAlgorithm::Sha256 => HashAlgorithm::Sha256,
                    DigestAlgorithm::Sha384 => HashAlgorithm::Sha384,
                    DigestAlgorithm::Sha512 => HashAlgorithm::Sha512,
                })
                .map_err(|source| pdf_oxide::Error::from(io::Error::other(source)))?;
            hasher.update(message);
            hasher.finalize()
        };
        let identifier = tagged(SEQUENCE, &tagged(OBJECT_IDENTIFIER, algorithm.oid()));
        let request = tagged(SEQUENCE, &[tagged(INTEGER, &[1]), tagged(SEQUENCE, &[identifier, tagged(OCTET_STRING, &imprint)].concat()), tagged(BOOLEAN, &[0xFF])].concat());
        let response = authority.client.post(authority.url.clone()).header(CONTENT_TYPE, "application/timestamp-query").body(request).send()?.error_for_status()?.bytes()?;
        let stamp = match element(&response).and_then(|(_, body, _)| element(body)).and_then(|(_, information, rest)| element(information).map(|(_, status, _)| (status, rest))) {
            Some(([0 | 1], rest)) => element(rest).map(|(whole, _, _)| whole).ok_or(PdfError::Timestamp(None)),
            Some(([status], _)) => Err(PdfError::Timestamp(Some(*status))),
            Some(_) | None => Err(PdfError::Timestamp(None)),
        }?;
        (Timestamp::from_der(stamp)?.message_imprint_ref() == imprint).ok_or(PdfError::Imprint).map(|()| stamp.to_vec())
    }
    let unvalidated = RevocationMaterial::default();
    let (pades, authority, material) = match level {
        Level::Basic => (PadesLevel::BB, None, &unvalidated),
        Level::Timestamp(authority) => (PadesLevel::BT, Some(authority), &unvalidated),
        Level::LongTerm { authority, material } => (PadesLevel::BLt, Some(authority), material),
        Level::Archival { authority, material } => (PadesLevel::BLta, Some(authority), material),
    };
    let algorithm = options.digest_algorithm;
    let timestamper = authority.map(|authority| move |message: &[u8]| token(authority, algorithm, message).map_err(|error| pdf_oxide::Error::from(io::Error::other(error))));
    let signed = sign_pdf_bytes_pades(
        &fielded(document, pades, material)?,
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
