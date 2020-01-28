using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OnBoardClientel.Functions
{
    [Table("Clients")]
   public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Industry { get; set; }
        public string Comment { get; set; }
        public string Url { get; set; }
        public DateTime? DocumentGenerated { get; set; }
        public DateTime? EmailSent { get; set; }
        public DateTime? DocumentReviewed { get; set; }
        public string DurableFunctionUrl { get; set; }
        public DateTime CreatedTime { get; set; }
        public string DocumentUrl { get; set; }

    }
}
