// File: src/components/CommentItem/CommentItem.jsx
import React, { useState } from 'react';
import {
  Card,
  Button,
  Collapse,
  Badge
} from 'react-bootstrap';
import CommentForm from '../CommentForm/CommentForm';
import './CommentItem.css';
import { commentService } from '../../services/api';

const CommentItem = ({ comment, onReplyAdded, onDeleted, level = 0 }) => {
  const [showReplyForm, setShowReplyForm] = useState(false);
  const [showReplies, setShowReplies] = useState(true);

  const handleReplyAdded = (newReply) => {
    setShowReplyForm(false);
    onReplyAdded(newReply);
  };

  const handleDelete = async () => {
    if (window.confirm('Are you sure you want to delete this comment?')) {
      try {
        await commentService.deleteComment(comment.id);
        onDeleted();
      } catch (err) {
        alert('Error deleting comment: ' + (err.message || 'Cannot delete if has replies'));
      }
    }
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const isImage = comment.file?.fileType === 'Image';
  const isTextFile = comment.file?.fileType === 'Text';
  const hasReplies = comment.replies && comment.replies.length > 0;

  return (
      <div
          className={`comment-item ${level > 0 ? 'comment-reply' : ''}`}
          data-level={level}
      >
        <Card className="mb-3 comment-card">
          <Card.Body>
            <div className="d-flex justify-content-between align-items-start mb-2">
              <div className="comment-header">
                <div className="d-flex align-items-center gap-2">
                  <strong className="comment-username">{comment.userName}</strong>
                  {level > 0 && (
                      <Badge bg="outline-primary" text="primary" className="reply-badge">
                        Reply
                      </Badge>
                  )}
                </div>
                <div className="comment-meta">
                  <small className="text-muted">
                    {comment.email} • {formatDate(comment.createdAt)}
                    {comment.homePage && (
                        <> • <a
                            href={comment.homePage}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="text-decoration-none"
                        >
                          Website
                        </a></>
                    )}
                  </small>
                </div>
              </div>
            </div>

            <div
                className="comment-text mt-2"
                dangerouslySetInnerHTML={{ __html: comment.textHtml || '' }}
            />

            {comment.file && comment.file.filePath && (
                <div className="mt-3 file-attachment">
                  {isImage && (
                      <div className="image-attachment">
                        <img
                            src={`${API_BASE_URL}${comment.file.filePath}`}  // Adjusted to use API_BASE_URL without /api/Files, assuming correct serving
                            alt="Attached image"
                            className="comment-image img-thumbnail"
                            style={{
                              maxWidth: comment.file.thumbnailPath ? '150px' : '320px',
                              maxHeight: comment.file.thumbnailPath ? '112px' : '240px'
                            }}
                        />
                        {comment.file.thumbnailPath && (
                            <div className="mt-1">
                              <a
                                  href={`${API_BASE_URL}${comment.file.filePath}`}
                                  target="_blank"
                                  rel="noopener noreferrer"
                                  className="btn btn-sm btn-outline-primary"
                              >
                                Open original
                              </a>
                            </div>
                        )}
                      </div>
                  )}
                  {isTextFile && (
                      <div className="text-attachment">
                        <a
                            href={`${API_BASE_URL}${comment.file.filePath}`}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="btn btn-sm btn-outline-primary"
                        >
                          📄 Download text file ({comment.file.fileName})
                        </a>
                      </div>
                  )}
                </div>
            )}

            <div className="comment-actions mt-3">
              {level < 5 && (
                  <Button
                      variant="outline-primary"
                      size="sm"
                      onClick={() => setShowReplyForm(!showReplyForm)}
                      className="me-2"
                  >
                    {showReplyForm ? 'Cancel' : 'Reply'}
                  </Button>
              )}

              {hasReplies && (
                  <Button
                      variant="outline-secondary"
                      size="sm"
                      onClick={() => setShowReplies(!showReplies)}
                      className="me-2"
                  >
                    {showReplies ? 'Hide replies' : `Show replies (${comment.replies.length})`}
                  </Button>
              )}

              <Button
                  variant="outline-danger"
                  size="sm"
                  onClick={handleDelete}
                  disabled={hasReplies}
              >
                Delete
              </Button>
            </div>

            <Collapse in={showReplyForm}>
              <div className="mt-3">
                <CommentForm
                    onCommentAdded={handleReplyAdded}
                    parentId={comment.id}
                    compact={true}
                />
              </div>
            </Collapse>
          </Card.Body>
        </Card>

        {hasReplies && showReplies && (
            <div className="comment-replies">
              {comment.replies.map(reply => (
                  <CommentItem
                      key={reply.id}
                      comment={reply}
                      onReplyAdded={onReplyAdded}
                      onDeleted={onDeleted}
                      level={level + 1}
                  />
              ))}
            </div>
        )}
      </div>
  );
};

export default CommentItem;