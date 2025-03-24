using Stanok.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stanok.DataAccess.Entities;

public class DeliveryEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid StanokId { get; set; }
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }
}