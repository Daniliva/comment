import React, { useState } from 'react';
import { Button, Collapse } from 'react-bootstrap';
import Lightbox from 'react-image-lightbox';
import CommentForm from '../CommentForm/CommentForm';
import CommentList from '../CommentList/CommentList';
import { commentService } from '../../services/api';
const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7002';

const CommentRow = ({ comment, onCommentAdded, onCommentDeleted, level = 0 }) => {
    const [showReplyForm, setShowReplyForm] = useState(false);
    const [isExpanded, setIsExpanded] = useState(false);
    const [isOpen, setIsOpen] = useState(false);

    const toggleExpand = () => setIsExpanded(!isExpanded);

    const handleDelete = async () => {
        if (window.confirm('Are you sure you want to delete this comment?')) {
            try {
                await commentService.deleteComment(comment.id);
                onCommentDeleted();
            } catch (err) {
                alert('Error deleting comment: ' + (err.message || 'Cannot delete if has replies'));
            }
        }
    };

    const handleReplySubmit = (newComment) => {
        setShowReplyForm(false);
        onCommentAdded(newComment);
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
        <React.Fragment>
            <tr className={`accordion-toggle ${isExpanded ? '' : 'collapsed'}`} onClick={toggleExpand} style={{ cursor: 'pointer' }}>
                <td>{comment.userName}</td>
                <td>{comment.email}</td>
                <td>{formatDate(comment.createdAt)}</td>
                <td>
                    <div dangerouslySetInnerHTML={{ __html: comment.textHtml || '' }} />
                    {comment.file && (
                        isImage ? (
                            <img
                                src={`${API_BASE_URL}${comment.file.thumbnailPath || comment.file.filePath}`}
                                alt="Attached image"
                                onClick={(e) => {
                                    e.stopPropagation();
                                    setIsOpen(true);
                                }}
                                style={{ maxWidth: '100px', cursor: 'pointer' }}
                            />
                        ) : (
                            <a href={`${API_BASE_URL}${comment.file.filePath}`} download>
                                Download {comment.file.fileName}{comment.file.fileExtension}
                            </a>
                        )
                    )}
                </td>
                <td>
                    {level < 5 && (
                        <Button
                            variant="outline-primary"
                            size="sm"
                            onClick={(e) => { e.stopPropagation(); setShowReplyForm(!showReplyForm); }}
                        >
                            {showReplyForm ? 'Cancel Reply' : 'Reply'}
                        </Button>
                    )}
                    <Button
                        variant="outline-danger"
                        size="sm"
                        onClick={(e) => { e.stopPropagation(); handleDelete(); }}
                        disabled={hasReplies}
                    >
                        Delete
                    </Button>
                </td>
            </tr>
            <tr className="hide-table-padding">
                <td colSpan={5}>
                    <Collapse in={isExpanded || showReplyForm}>
                        <div className="p-3">
                            {showReplyForm && level < 5 && (
                                <CommentForm
                                    onCommentAdded={handleReplySubmit}
                                    parentId={comment.id}
                                    compact={true}
                                />
                            )}
                            {hasReplies && (
                                <CommentList
                                    comments={comment.replies}
                                    loading={false}
                                    pagination={{ totalPages: 1, page: 1, totalCount: comment.replies.length, pageSize: comment.replies.length }}
                                    sortConfig={{}}
                                    onPageChange={() => {}}
                                    onSortChange={() => {}}
                                    onCommentAdded={onCommentAdded}
                                    onCommentDeleted={onCommentDeleted}
                                    isNested={true}
                                />
                            )}
                        </div>
                    </Collapse>
                </td>
            </tr>
            {isImage && isOpen && (
                <Lightbox
                    mainSrc={`${API_BASE_URL}${comment.file.filePath}`}
                    onCloseRequest={() => setIsOpen(false)}
                />
            )}
        </React.Fragment>
    );
};

export default CommentRow;