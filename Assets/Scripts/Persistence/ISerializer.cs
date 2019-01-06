﻿using System.IO;

namespace Persistence
{
    public interface ISerializer
    {
        void Serialize(Stream stream, object graph);
        object Deserialize(Stream stream);
    }
}
