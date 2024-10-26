using System;

namespace MasstransitSaga.Core.Environments;

public interface IRedisSettings
{
    string GetRedisConfiguration();
}
