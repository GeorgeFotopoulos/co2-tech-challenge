using System.Threading;
using System.Collections.Generic;

namespace TechChallenge.Emissions.Api.Data;

public interface IEmissionsRepository
{
    IAsyncEnumerable<Emission> GetAsync(
        long from,
        long to,
        CancellationToken cancellationToken);
}