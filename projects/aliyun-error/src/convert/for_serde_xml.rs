
use crate::{AliError, AliErrorKind};

impl From<serde_xml_rs::Error> for AliError {
    fn from(value: serde_xml_rs::Error) -> Self {
        let kind = AliErrorKind::DecoderError { format: "xml".to_string(), message: value.to_string() };
        Self { kind: Box::new(kind) }
    }
}
