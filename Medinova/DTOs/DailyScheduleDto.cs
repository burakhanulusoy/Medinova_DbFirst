using Medinova.Models;
using System;
using System.Collections.Generic;

namespace Medinova.DTOs
{
    public class DailyScheduleDto
    {
        public DateTime Date { get; set; }
        public List<Appointment> Appointments { get; set; }
    }
}