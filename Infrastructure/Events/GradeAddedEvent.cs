using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Events;

public class GradeAddedEvent
{
    public Guid GradeId { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public Guid TeacherId { get; set; }
    public int GradeValue { get; set; }
    public DateTime Timestamp { get; set; }
}
