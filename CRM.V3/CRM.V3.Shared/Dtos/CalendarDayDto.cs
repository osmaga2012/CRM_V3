namespace CRM.V3.Shared.Dtos
{
    public class CalendarDayDto
    {
        public DateTime Fecha { get; set; }
        public bool EsDelMesActual { get; set; }

        // Puedes usar una lista de eventos resumidos o una clase propia
        public List<CalendarioEventoDto> Eventos { get; set; } = new();
    }
}
