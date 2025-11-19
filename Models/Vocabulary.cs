using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LushEnglishAPI.Models;

namespace LushEnglishAPI.Models
{
    public class Vocabulary
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [Required]
        public Guid TopicId { get; set; }

        [Required, MaxLength(255)]
        public string Word { get; set; }

        [MaxLength(255)]
        public string Phonetic { get; set; }

        [Required]
        public string Meaning { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Topic Topic { get; set; }
    }
}
