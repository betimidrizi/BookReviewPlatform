namespace BookReviewPlatform.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string UserId { get; set; }  // To track which user left the review
        public string Comment { get; set; }  // The review text

        public int BookId { get; set; }  // Link to the book
        public Book Book { get; set; }
    }
}
