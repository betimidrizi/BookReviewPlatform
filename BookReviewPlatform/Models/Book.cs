namespace BookReviewPlatform.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public List<Review>? Reviews { get; set; }
        public List<BookReaction>? Reactions { get; set; }

    }


}
