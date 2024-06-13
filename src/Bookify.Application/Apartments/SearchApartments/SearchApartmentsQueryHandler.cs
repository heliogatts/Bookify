using Bookify.Application.Abstractions.Data;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Bookings;
using Dapper;

namespace Bookify.Application.Apartments.SearchApartments;

public class SearchApartmentsQueryHandler : IQueryHandler<SearchApartmentsQuery, IReadOnlyList<ApartmentResponse>>
{
    private static readonly int[] ActiveBookingStatuses = new[]
    {
        (int)BookingStatus.Reserved,
        (int)BookingStatus.Confirmed,
        (int)BookingStatus.Completed
    };
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public SearchApartmentsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<IReadOnlyList<ApartmentResponse>>> Handle(SearchApartmentsQuery request, CancellationToken cancellationToken)
    {
        if (request.StartDate > request.EndDate)
        {
            return new List<ApartmentResponse>();
        }
        using var connection = _sqlConnectionFactory.CreateConnection();
        const string sql = """
                           SELECT
                               a.Id AS ApartmentId,
                               a.name AS Name,
                               a.Description AS Description,
                               a.price_amount AS Price,
                               a.price_currency AS PriceCurrency,
                               a.address_country AS Country,
                               a.address_state AS State,
                               a.address_zip_code AS ZipCode,
                               a.address_city AS City,
                               a.address_street AS Street
                           FROM Apartments AS a
                           WHERE NOT EXISTS 
                           (
                               SELECT 1
                               FROM Bookings AS b
                               WHERE 
                                   b.apartment_id = a.id AND
                                   b.duration_start <= @EndDate AND
                                   b.duration_end >= @StartDate AND
                                   b.status = ANY(@ActiveBookingStatuses)
                           )
                           """;

        var apartments = await connection
            .QueryAsync<ApartmentResponse, AddressResponse, ApartmentResponse>(
                sql,
                (apartment, address) =>
                {
                    apartment.Address = address;

                    return apartment;
                }, new
                {
                    request.StartDate,
                    request.EndDate,
                    ActiveBookingStatuses
                }, splitOn: "Country");
        return apartments.ToList();
    }

}