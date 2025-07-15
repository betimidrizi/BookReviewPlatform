using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookReviewPlatform.Models
{
    public class BookReaction
    {
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }
        public Book Book { get; set; }

        [Required]
        public string UserId { get; set; } // FK to ASP.NET Identity User

        [Required]
        public bool IsLike { get; set; }   // true = Like, false = Dislike
    }
}
