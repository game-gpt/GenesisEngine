

pub mod client;
pub mod consumer;
#[cfg(test)]
pub mod devtool;
pub mod error;
pub mod options;
pub mod queue;
pub mod queue_manager;

/// 消息操作 API，包括消息的发送、接收、删除、修改可见性等操作
/// <https://help.aliyun.com/document_detail/140735.html>
pub type Queue = queue::Queue;
pub use crate::{client::AlibabaMNS, queue_manager::QueueManager};
