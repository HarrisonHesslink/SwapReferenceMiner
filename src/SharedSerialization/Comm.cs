﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SharedSerialization
{
    [SerializableAttribute]
    public class Job
    {
        private static Random rnd = new Random((int)DateTime.Now.Ticks);

        public DateTime timestamp;
        public UInt64 nonce, height, difficulty, jobID;
        public UInt64 hnonce;
        public UInt64 k0;
        public UInt64 k1;
        public UInt64 k2;
        public UInt64 k3;
        public string pre_pow;

        public int graphAttempts = 0;
        public long trimTime = 0;

        public void MutateJob()
        {
            graphAttempts++;
            var header = GetHeaderBytes();
            hnonce = (UInt64)(long)rnd.Next() | ((UInt64)(long)rnd.Next() << 32);
            var bytes = BitConverter.GetBytes(hnonce).Reverse().ToArray();
            header = header.Concat(bytes).ToArray();
            var hash = new Crypto.Blake2B(256);
            byte[] blaked = hash.ComputeHash(header);
            k0 = BitConverter.ToUInt64(blaked, 0);
            k1 = BitConverter.ToUInt64(blaked, 8);
            k2 = BitConverter.ToUInt64(blaked, 16);
            k3 = BitConverter.ToUInt64(blaked, 24);
        }

        public byte[] GetHeaderBytes()
        {
            return Enumerable.Range(0, pre_pow.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(pre_pow.Substring(x, 2), 16))
                     .ToArray();
        }
    }
    [SerializableAttribute]
    public struct Edge
    {
        public Edge(UInt32 U, UInt32 V)
        {
            Item1 = U;
            Item2 = V;
        }

        public UInt32 Item1;
        public UInt32 Item2;
    }
    [SerializableAttribute]
    public class Solution
    {
        public Job job;
        public List<Edge> edges;
        public UInt32[] nonces;

        public ulong[] GetUlongEdges()
        {
            return edges.Select(e => (ulong)e.Item1 | (((ulong)e.Item2) << 32)).ToArray();
        }

        /*
         * 	/// Difficulty achieved by this proof with given scaling factor
	        fn scaled_difficulty(&self, scale: u64) -> u64 {
		        let diff = ((scale as u128) << 64) / (self.hash().to_u64() as u128);
		        min(diff, <u64>::max_value() as u128) as u64
	        }
         * */
        public bool CheckDifficulty(ulong target, out ulong diff)
        {
            // not working !!!
            var solB = nonces.Select(x => BitConverter.GetBytes(x).Reverse().ToArray()).SelectMany(x => x).ToArray<Byte>();
            var hash = new Crypto.Blake2B(256);
            UInt64 blaked = BitConverter.ToUInt64(hash.ComputeHash(solB).Reverse().ToArray(), 24);
            UInt64 div = (UInt64.MaxValue / blaked);
            diff = (ulong)div;
            return div >= target;
        }
    }

    [SerializableAttribute]
    public class GpuDevice
    {
        public int id;
        public string name;
        public long memory;
    }
    [SerializableAttribute]
    public class GpuDevicesMessage
    {
        public List<GpuDevice> devices;
    }
    [SerializableAttribute]
    public enum WorkerType : int
    {
        AMD = 0,
        NVIDIA = 20
    }

    [SerializableAttribute]
    public class LogMessage
    {
        public DateTime time;
        public LogLevel level;
        public string message;
        public Exception ex;
    }

    [SerializableAttribute]
    public enum LogLevel:int
    {
        Debug,
        Info,
        Warning,
        Error
    }
}