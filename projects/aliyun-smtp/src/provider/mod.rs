use crate::EmailSender;
use aliyun_error::party_3rd::lettre::{
    transport::smtp::{authentication::Credentials, response::Response}, Message,
    SmtpTransport,
    Transport,
};

mod aliyun;

pub use self::aliyun::AlibabaSMTP;
