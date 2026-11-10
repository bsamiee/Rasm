use core::iter;
use std::sync::Arc;

use hayro_syntax::object::dict::keys::{DEST_OUTPUT_PROFILE, INFO, METADATA, N, OUTPUT_CONDITION, OUTPUT_CONDITION_IDENTIFIER, OUTPUT_INTENT, OUTPUT_INTENTS, S, TYPE};
use krilla::SerializeSettings;
use krilla::configure::{Accessibility, Archival, ConfigurationBuilder, PdfVersion, ValidationError};
use lopdf::{Dictionary, Object, Stream, dictionary, text_string};
use moxcms::{ColorProfile, ProfileText};
use pdf_oxide::compliance::PdfXLevel;
use xot::{NameId, Node, Xot};

use crate::{GTS_PDFX_VERSION, OutputIntent, PDFXID_NAMESPACE, PdfError, Refusal};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Standards {
    pub archival: Option<Archival> = None,
    pub accessibility: Option<Accessibility> = None,
    pub press: Option<Press> = None,
}

#[derive(Debug, Clone, derive_more::AsRef)]
pub struct Press {
    #[as_ref]
    level: PdfXLevel,
    #[as_ref]
    intent: OutputIntent,
}

impl Press {
    pub fn new(level: PdfXLevel, intent: OutputIntent) -> Result<Self, PdfError> {
        (level.allows_transparency() && level.allows_layers() && !level.allows_external_icc() && !level.allows_external_graphics()).ok_or(PdfError::Press(level)).map(|()| Self { level, intent })
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn settings(standards: &Standards) -> Result<SerializeSettings, PdfError> {
    let stated = |level: PdfXLevel| {
        [PdfVersion::Pdf14, PdfVersion::Pdf15, PdfVersion::Pdf16, PdfVersion::Pdf17, PdfVersion::Pdf20]
            .into_iter()
            .find(|version| version.as_str().strip_prefix("PDF ") == Some(level.required_pdf_version()))
            .ok_or(PdfError::Press(level))
    };
    let dated = standards.press.as_ref().map(|press| stated(press.level)).transpose()?.into_iter().fold(ConfigurationBuilder::new(), ConfigurationBuilder::with_version);
    let archived = standards.archival.into_iter().fold(dated, ConfigurationBuilder::with_archival_validator);
    let configuration = standards.accessibility.into_iter().fold(archived, ConfigurationBuilder::with_accessibility_validator).finish().map_err(PdfError::Configuration)?;
    Ok(SerializeSettings {
        configuration,
        no_device_cs: standards.press.is_some(),
        cmyk_profile: standards.press.as_ref().map(|press| press.intent.icc.clone()),
        enable_tagging: standards.accessibility.is_some() || configuration.validators().prohibits(&ValidationError::MissingTagging).is_some(),
        render_svg_glyph_fn: krilla_svg::render_svg_glyph,
        ..SerializeSettings::default()
    })
}

pub(crate) fn close(structure: &mut lopdf::Document, standards: &Standards) -> Result<(), PdfError> {
    const CUSTOM_CONDITION: &str = "Custom";
    const TRAPPED: &str = "Trapped";
    const UNTRAPPED: &str = "False";
    let older = standards.archival.is_some_and(|level| level.max() < PdfVersion::Pdf20);
    let metadata = |structure: &mut lopdf::Document, standards: &Standards| -> Result<(), PdfError> {
        const RDF_NAMESPACE: &str = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        const PDF_NAMESPACE: &str = "http://ns.adobe.com/pdf/1.3/";
        const EXTENSION_NAMESPACE: &str = "http://www.aiim.org/pdfa/ns/extension/";
        const PDFX_EXTENSION_SCHEMA: &str = r#"<rdf:li xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#" xmlns:pdfaSchema="http://www.aiim.org/pdfa/ns/schema#"
    xmlns:pdfaProperty="http://www.aiim.org/pdfa/ns/property#" rdf:parseType="Resource">
    <pdfaSchema:schema>PDF/X Identification</pdfaSchema:schema>
    <pdfaSchema:namespaceURI>http://www.npes.org/pdfx/ns/id/</pdfaSchema:namespaceURI>
    <pdfaSchema:prefix>pdfxid</pdfaSchema:prefix>
    <pdfaSchema:property>
        <rdf:Seq>
            <rdf:li rdf:parseType="Resource">
                <pdfaProperty:category>internal</pdfaProperty:category>
                <pdfaProperty:description>Version of ISO 15930 (PDF/X)</pdfaProperty:description>
                <pdfaProperty:name>GTS_PDFXVersion</pdfaProperty:name>
                <pdfaProperty:valueType>Text</pdfaProperty:valueType>
            </rdf:li>
        </rdf:Seq>
    </pdfaSchema:property>
</rdf:li>"#;
        let element = |xot: &Xot, root: Node, name: NameId| {
            xot.descendants(root)
                .find(|&node| xot.element(node).is_some_and(|held| held.name() == name))
                .ok_or_else(|| PdfError::Named { name: Arc::from(xot.name_ns_str(name).0), refusal: Refusal::Metadata })
        };
        let stream = structure.catalog()?.get(METADATA)?.as_reference()?;
        let packet = structure.get_object(stream)?.as_stream()?.get_plain_content()?;
        let mut xot = Xot::new();
        let document = xot.parse(str::from_utf8(&packet)?).map_err(xot::Error::from)?;
        let rdf = xot.add_namespace(RDF_NAMESPACE);
        let description_name = xot.add_name_ns("Description", rdf);
        let description = element(&xot, document, description_name)?;
        standards.press.iter().flat_map(|held| [("pdfxid", PDFXID_NAMESPACE, GTS_PDFX_VERSION, held.level.xmp_version()), ("pdf", PDF_NAMESPACE, TRAPPED, UNTRAPPED)]).try_for_each(
            |(prefix, namespace, name, value)| {
                let (uri, declared) = (xot.add_namespace(namespace), xot.add_prefix(prefix));
                xot.set_namespace(description, declared, uri);
                let property = xot.add_name_ns(name, uri);
                let added = xot.new_element(property);
                xot.append(description, added)?;
                xot.append_text(added, value)
            },
        )?;
        if standards.press.is_some() && older {
            let extension = xot.add_namespace(EXTENSION_NAMESPACE);
            let schemas_name = xot.add_name_ns("schemas", extension);
            let bag_name = xot.add_name_ns("Bag", rdf);
            let schemas = element(&xot, document, schemas_name)?;
            let bag = element(&xot, schemas, bag_name)?;
            let fragment = xot.parse(PDFX_EXTENSION_SCHEMA).map_err(xot::Error::from)?;
            let schema = xot.document_element(fragment)?;
            xot.detach(schema)?;
            xot.append(bag, schema)?;
        }
        let written = xot.to_string(document)?;
        structure.get_object_mut(stream)?.as_stream_mut()?.set_plain_content(written.into_bytes());
        Ok(())
    };
    if let Some(press) = &standards.press {
        let data: &[u8] = press.intent.as_ref();
        let profile: &ColorProfile = press.intent.as_ref();
        let condition = match &profile.description {
            Some(ProfileText::PlainString(text)) => text.as_str(),
            Some(ProfileText::Localizable(strings)) => strings.first().map_or(CUSTOM_CONDITION, |string| string.value.as_str()),
            Some(ProfileText::Description(description)) => description.ascii_string.as_str(),
            None => CUSTOM_CONDITION,
        };
        let mut stream = Stream::new(dictionary! { N => 4 }, data.to_vec());
        stream.compress()?;
        let destination = structure.try_add_object(Object::Stream(stream))?;
        let described = [
            (OUTPUT_CONDITION_IDENTIFIER, text_string(CUSTOM_CONDITION)),
            (OUTPUT_CONDITION, text_string(condition)),
            (INFO, text_string(condition)),
            (DEST_OUTPUT_PROFILE, Object::Reference(destination)),
        ];
        let catalog = structure.catalog()?;
        let held = catalog.as_hashmap().get(OUTPUT_INTENTS).map(|value| structure.dereference(value)?.1.as_array().cloned()).transpose()?.unwrap_or_default();
        held.iter()
            .map(Object::as_reference)
            .try_collect::<Vec<_>>()?
            .into_iter()
            .try_for_each(|intent| structure.get_dictionary_mut(intent).map(|dictionary| described.iter().for_each(|(key, value)| dictionary.set(*key, value.clone()))))?;
        let press_intent = iter::chain([(TYPE, Object::Name(OUTPUT_INTENT.to_vec())), (S, Object::from("GTS_PDFX"))], described).collect::<Dictionary>();
        structure.catalog_mut()?.set(OUTPUT_INTENTS, iter::chain(held, [Object::from(press_intent)]).collect::<Vec<_>>());
        if standards.archival.is_none() || older {
            let entry = structure.trailer.as_hashmap().get(INFO).map(Object::as_reference).transpose()?.map_or_else(
                || {
                    let created = structure.try_add_object(Dictionary::new())?;
                    structure.trailer.set(INFO, created);
                    Ok::<_, lopdf::Error>(created)
                },
                Ok,
            )?;
            let information = structure.get_dictionary_mut(entry)?;
            information.set(GTS_PDFX_VERSION, text_string(press.level.gts_pdfx_version()));
            information.set(TRAPPED, UNTRAPPED);
        }
    }
    if standards.press.is_some() { metadata(structure, standards) } else { Ok(()) }
}
