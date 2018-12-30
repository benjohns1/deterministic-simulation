using Simulation.State;
using System;
using System.Collections.Generic;
using EntityID = System.UInt64;
using ArchetypeName = System.String;
using System.IO;
using Simulation.Serialization;

[Serializable]
public class SerializableGameData
{
    public Snapshot Snapshot { get; }
    public Dictionary<EntityID, ArchetypeName> Archetypes { get; }

    public SerializableGameData(Snapshot snapshot, Dictionary<EntityID, ArchetypeName> archetypes)
    {
        Snapshot = snapshot;
        Archetypes = archetypes;
    }

    internal void Serialize(Stream stream)
    {
        Serializer serializer = new Serializer();
        serializer.Serialize(stream, this);
    }

    internal static SerializableGameData Deserialize(Stream stream)
    {
        Serializer serializer = new Serializer();
        return (SerializableGameData)serializer.Deserialize(stream);
    }
}