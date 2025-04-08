﻿namespace PocCQRS.Infrastructure.Persistence.Cache.Interfaces;

public interface IManageCacheClient
{
    TimeSpan? GetTimeToLive(string key);
    void Refresh(string key, TimeSpan? expiry = null);
}