using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tienda.src.Application.Domain.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        public required int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public required int Quantity { get; set; }
        public required string TitleAtMoment { get; set; }
        public required int PriceAtMoment { get; set; }
        public required string DescriptionAtMoment { get; set; }
        public required string ImageUrlAtMoment { get; set; }
        public required int DiscountAtMoment { get; set; }
    }
}