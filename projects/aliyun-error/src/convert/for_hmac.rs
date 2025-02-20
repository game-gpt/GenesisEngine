use hmac::digest::InvalidLength;
use crate::{AliError, AliErrorKind};

impl From<InvalidLength> for AliError {
    fn from(value: InvalidLength) -> Self {
        let kind = AliErrorKind::DecoderError { format: "hmac".to_string(), message: value.to_string() };
        Self { kind: Box::new(kind) }
    }
}