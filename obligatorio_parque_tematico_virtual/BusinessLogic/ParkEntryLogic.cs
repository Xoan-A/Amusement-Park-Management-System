using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.In;
using Models.Out;

namespace BusinessLogic
{
    public class ParkEntryLogic : IParkEntryLogic
    {
        private readonly IUserRepository _userRepository;
        private readonly IAttractionRepository _attractionRepository;
        private readonly ITicketLogic _ticketLogic;
        private readonly IEventRepository _eventRepository;
        private readonly IDailyScoreLogic _dailyScoreLogic;
        private readonly IDateTimeLogic _dateTimeLogic;

        public ParkEntryLogic(
            IUserRepository userRepository,
            IAttractionRepository attractionRepository,
            ITicketLogic ticketLogic,
            IEventRepository eventRepository,
            IDailyScoreLogic dailyScoreLogic,
            IDateTimeLogic dateTimeLogic)
        {
            _userRepository = userRepository;
            _attractionRepository = attractionRepository;
            _ticketLogic = ticketLogic;
            _eventRepository = eventRepository;
            _dailyScoreLogic = dailyScoreLogic;
            _dateTimeLogic = dateTimeLogic;
        }

        public void RegisterEntry(Guid attractionId, RegisterEntryRequest request)
        {
            Guid? qr = request.Qr;
            Guid? nfc = request.NFC;
            Guid? eventId = request.EventId;
            DateTime enterDate = _dateTimeLogic.GetCurrentDateTime();

            if (qr == null && nfc == null)
                throw new ArgumentException("QR code or NFC must be provided.");

            Attraction attraction = _attractionRepository.GetById(attractionId);
            if (attraction == null)
                throw new ArgumentException("Attraction not found.");

            bool isValidTicket = _ticketLogic.ValidateTicket(qr, nfc, enterDate, eventId, attractionId);
            if (!isValidTicket)
                throw new ArgumentException("User does not have a valid ticket.");

            TicketResponse ticket;
            Guid userId;
            if (qr != null)
            {
                ticket = _ticketLogic.GetTicketByQRCode(qr.Value);
                userId = ticket.VisitorId;
            }
            else
            {
                userId = nfc.Value;
            }
            
            User user = _userRepository.GetById(userId);
            if (user == null)
                throw new ArgumentException("User not found.");

            if (attraction.CurrentCapacity < attraction.MaxCapacity)
            {
                user.RegisterEntry(attraction, enterDate);
                attraction.CurrentCapacity++;
                _userRepository.Update(user);
                _attractionRepository.Update(attraction);
            }
            else
                throw new ArgumentException("Attraction is at full capacity.");

            Event even = _eventRepository.GetEventByAttractionAndDate(attractionId, enterDate.Date);

            _dailyScoreLogic.AddScoreToUser(user, attraction, enterDate, even);
        }

        public void RegisterExit(Guid attractionId, RegisterExitRequest request)
        {
            Guid userId = request.userId;
            DateTime exitDate = _dateTimeLogic.GetCurrentDateTime();

            User user = _userRepository.GetById(userId);
            if (user == null)
                throw new ArgumentException("User not found.");

            Attraction attraction = _attractionRepository.GetById(attractionId);
            if (attraction == null)
                throw new ArgumentException("Attraction not found.");

            user.RegisterExit(attraction, exitDate);
            _userRepository.Update(user);

            if (attraction.CurrentCapacity > 0)
            {
                attraction.CurrentCapacity--;
                _attractionRepository.Update(attraction);
            }
        }
    }
}
