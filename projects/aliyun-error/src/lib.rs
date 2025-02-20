#![deny(missing_debug_implementations, missing_copy_implementations)]
#![warn(missing_docs, rustdoc::missing_crate_level_docs)]
#![doc = include_str!("../readme.md")]
#![doc(html_logo_url = "https://raw.githubusercontent.com/oovm/shape-rs/dev/projects/images/Trapezohedron.svg")]
#![doc(html_favicon_url = "https://raw.githubusercontent.com/oovm/shape-rs/dev/projects/images/Trapezohedron.svg")]

mod convert;

mod display;

/// 常用第三方库
pub mod party_3rd {
    #[cfg(feature = "chrono")]
    pub use chrono;
    #[cfg(feature = "hmac")]
    pub use hmac::{Hmac, Mac};
    #[cfg(feature = "lettre")]
    pub use lettre;
    #[cfg(feature = "reqwest")]
    pub use reqwest;
}

/// The result type of this crate.
pub type Result<T> = std::result::Result<T, AliError>;

/// A boxed error kind, wrapping an [AliErrorKind].
#[derive(Clone)]
pub struct AliError {
    kind: Box<AliErrorKind>,
}

/// 枚举定义阿里云服务中可能发生的错误类型
#[derive(Debug, Clone)]
pub enum AliErrorKind {
    /// 服务器错误
    ServiceError {
        /// 错误信息
        message: String,
    },
    /// 网络错误
    NetworkError {
        /// 错误信息
        message: String,
    },
    /// 编码错误
    EncoderError {
        /// 错误编码格式
        format: String,
        /// 错误信息
        message: String,
    },
    /// 解码错误
    DecoderError {
        /// 错误解码格式
        format: String,
        /// 错误信息
        message: String,
    },
    /// 自定义错误
    CustomError {
        /// 错误信息
        message: String,
    },
    /// 未知错误
    UnknownError,
}

impl AliError {
    /// 创建一个网络错误
    pub fn network_error(message: impl Into<String>) -> Self {
        AliErrorKind::NetworkError { message: message.into() }.into()
    }
    /// 创建一个自定义错误
    pub fn custom_error(message: impl Into<String>) -> Self {
        AliError { kind: Box::new(AliErrorKind::CustomError { message: message.into() }) }
    }
}
