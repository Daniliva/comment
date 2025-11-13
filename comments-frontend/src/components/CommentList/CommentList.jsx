// File: src/components/CommentList/CommentList.jsx
import React from 'react';
import {
    Table,
    Spinner,
    Form,
    Row,
    Col,
    Pagination
} from 'react-bootstrap';
import CommentRow from '../CommentRow/CommentRow';

const CommentList = ({
                         comments,
                         loading,
                         pagination,
                         sortConfig,
                         onPageChange,
                         onSortChange,
                         onCommentAdded,
                         onCommentDeleted,
                         isNested = false
                     }) => {

    const handleSortChange = (e) => {
        const [sortBy, sortOrder] = e.target.value.split('-');
        onSortChange(sortBy, sortOrder === 'desc');
    };

    const handlePageChange = (page) => {
        onPageChange(page);
    };

    const renderPaginationItems = () => {
        const items = [];
        const { page, totalPages } = pagination;

        items.push(
            <Pagination.Item
                key={1}
                active={1 === page}
                onClick={() => handlePageChange(1)}
            >
                1
            </Pagination.Item>
        );

        let startPage = Math.max(2, page - 2);
        let endPage = Math.min(totalPages - 1, page + 2);

        if (startPage > 2) {
            items.push(<Pagination.Ellipsis key="start-ellipsis" />);
        }

        for (let i = startPage; i <= endPage; i++) {
            items.push(
                <Pagination.Item
                    key={i}
                    active={i === page}
                    onClick={() => handlePageChange(i)}
                >
                    {i}
                </Pagination.Item>
            );
        }

        if (endPage < totalPages - 1) {
            items.push(<Pagination.Ellipsis key="end-ellipsis" />);
        }

        if (totalPages > 1) {
            items.push(
                <Pagination.Item
                    key={totalPages}
                    active={totalPages === page}
                    onClick={() => handlePageChange(totalPages)}
                >
                    {totalPages}
                </Pagination.Item>
            );
        }

        return items;
    };

    if (loading && comments.length === 0) {
        return (
            <div className="text-center py-5">
                <Spinner animation="border" role="status" variant="primary">
                    <span className="visually-hidden">Loading...</span>
                </Spinner>
                <div className="mt-2">Loading comments...</div>
            </div>
        );
    }

    const tableContent = (
        <Table striped bordered hover responsive>
            {!isNested && (
                <thead>
                <tr>
                    <th>User Name</th>
                    <th>E-mail</th>
                    <th>Date</th>
                    <th>Text</th>
                    <th>Actions</th>
                </tr>
                </thead>
            )}
            <tbody>
            {comments.length === 0 ? (
                <tr>
                    <td colSpan={5} className="text-center py-5">
                        <div className="mb-3">📝</div>
                        <h6>No comments yet</h6>
                        <p>Be the first to leave a comment!</p>
                    </td>
                </tr>
            ) : (
                comments.map(comment => (
                    <CommentRow
                        key={comment.id}
                        comment={comment}
                        onCommentAdded={onCommentAdded}
                        onCommentDeleted={onCommentDeleted}
                        level={isNested ? 1 : 0}
                    />
                ))
            )}
            </tbody>
        </Table>
    );

    return (
        <div className="comment-list">
            {!isNested && (
                <Row className="align-items-center mb-3">
                    <Col>
                        <h5 className="mb-0">
                            Comments
                            {pagination.totalCount > 0 && (
                                <span className="text-muted"> ({pagination.totalCount})</span>
                            )}
                        </h5>
                    </Col>
                    <Col md="auto">
                        <Form.Select
                            size="sm"
                            value={`${sortConfig.sortBy}-${sortConfig.sortDescending ? 'desc' : 'asc'}`}
                            onChange={handleSortChange}
                        >
                            <option value="CreatedAt-desc">Newest first</option>
                            <option value="CreatedAt-asc">Oldest first</option>
                            <option value="UserName-asc">By username (A-Z)</option>
                            <option value="UserName-desc">By username (Z-A)</option>
                            <option value="Email-asc">By email (A-Z)</option>
                            <option value="Email-desc">By email (Z-A)</option>
                        </Form.Select>
                    </Col>
                </Row>
            )}

            {tableContent}

            {!isNested && pagination.totalPages > 1 && (
                <div className="d-flex justify-content-center mt-3">
                    <Pagination>
                        <Pagination.First
                            disabled={pagination.page === 1}
                            onClick={() => handlePageChange(1)}
                        />
                        <Pagination.Prev
                            disabled={pagination.page === 1}
                            onClick={() => handlePageChange(pagination.page - 1)}
                        />

                        {renderPaginationItems()}

                        <Pagination.Next
                            disabled={pagination.page === pagination.totalPages}
                            onClick={() => handlePageChange(pagination.page + 1)}
                        />
                        <Pagination.Last
                            disabled={pagination.page === pagination.totalPages}
                            onClick={() => handlePageChange(pagination.totalPages)}
                        />
                    </Pagination>
                </div>
            )}
        </div>
    );
};

export default CommentList;