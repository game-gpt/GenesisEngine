use super::*;
use aliyun_error::AliError;
use aliyun_error::party_3rd::lettre::message::Mailbox;
use std::{borrow::Cow, str::FromStr};

#[derive(Clone, Debug)]
pub struct AlibabaSMTP {
    smtp: SmtpTransport,
    company: Cow<'static, str>,
    sender: Mailbox,
}

impl AlibabaSMTP {
    pub fn login(username:  impl Into<String>, password:  impl Into<String>) -> Result<Self, AliError> {
        let username = username.into();
        let creds = Credentials::new(username.clone(), password.into());
        let mailer = SmtpTransport::relay("smtpdm.aliyun.com")?;
        let sender = Mailbox::from_str(&username)?;
        Ok(Self { smtp: mailer.credentials(creds).build(), company: Cow::Borrowed(""), sender })
    }
    pub fn with_company(mut self, company: &str) -> Self {
        self.company = Cow::Owned(company.to_string());
        self
    }
    pub fn with_sender(mut self, name: &str) -> Self {
        self.sender.name = Some(name.to_string());
        self
    }
}

impl EmailSender for AlibabaSMTP {
    fn send_message(&self, message: &Message) -> Result<Response, AliError> {
        Ok(self.smtp.send(message)?)
    }

    fn company_name(&self) -> Cow<str> {
        Cow::Borrowed(self.company.as_ref())
    }

    fn sender_mail(&self) -> Mailbox {
        self.sender.clone()
    }
}
