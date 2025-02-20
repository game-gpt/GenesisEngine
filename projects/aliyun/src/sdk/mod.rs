use crate::{AlibabaSDK, Result};
#[cfg(feature = "aliyun-dns")]
use aliyun_dns::AlibabaDNS;
#[cfg(feature = "aliyun-mns")]
use aliyun_mns::AlibabaMNS;
#[cfg(feature = "aliyun-oss")]
use aliyun_oss::oss::AlibabaOSS;
#[cfg(feature = "aliyun-sms")]
use aliyun_sms::aliyun::AlibabaSMS;
#[cfg(feature = "aliyun-smtp")]
use aliyun_smtp::AlibabaSMTP;

impl AlibabaSDK {
    /// 创建 OSS 实例
    #[cfg(feature = "aliyun-oss")]
    pub fn oss(&self, endpoint: impl Into<String>, bucket: impl Into<String>) -> AlibabaOSS {
        AlibabaOSS {
            access_key: self.access_key.clone(),
            access_secret: self.access_secret.clone(),
            endpoint: endpoint.into(),
            bucket: bucket.into(),
        }
    }
    /// 创建 SMS 实例
    #[cfg(feature = "aliyun-sms")]
    pub fn sms(&self) -> AlibabaSMS {
        AlibabaSMS { access_key: &self.access_key, access_secret: &self.access_secret }
    }
    /// 创建 DNS 实例
    #[cfg(feature = "aliyun-dns")]
    pub fn dns(&self) -> AlibabaDNS {
        AlibabaDNS { access_key: self.access_key.clone(), access_secret: self.access_secret.clone() }
    }
    /// 创建 DNS 实例
    #[cfg(feature = "aliyun-dns")]
    pub fn mns(&self, endpoint: &str) -> AlibabaMNS {
        AlibabaMNS {
            endpoint: endpoint.to_string(),
            access_key: self.access_key.to_string(),
            sec: self.access_secret.to_string(),
        }
    }
    /// 创建 SMTP 实例
    #[cfg(feature = "aliyun-smtp")]
    pub fn smtp(&self, endpoint: impl Into<String>, bucket: impl Into<String>) -> Result<AlibabaSMTP> {
        AlibabaSMTP::login(endpoint, bucket)
    }
}
