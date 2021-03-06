using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

// ChunkManager: Controls spawning and destroying chunks.

namespace Uniblocks
{
    public class ChunkManager : MonoBehaviour
    {
        public GameObject ChunkObject; // Chunk prefab

        [SerializeField]
        Transform _chunkContainer;


        // chunk lists
        public static Dictionary<string, Chunk> Chunks;

        static List<Chunk> ChunkUpdateQueue; // stores chunks ordered by update priority. Processed in the ProcessChunkQueue loop
        static List<Chunk> ChunksToDestroy; // chunks to be destroyed at the end of SpawnChunks

        public static int SavesThisFrame;


        // global flags
        public static bool SpawningChunks; // true if the ChunkManager is currently spawning chunks

        public static bool StopSpawning; // when true, the current SpawnChunks sequence is aborted (and this is set back to false afterwards)
        public static bool Initialized;


        // local flags
        bool _isDone;

        protected Index LastRequest;    
        float targetFrameDuration;
        Stopwatch frameStopwatch;

        public bool SpawnQueueIsEmpty { get; private set; }  

        
        void Awake()
        {
            SpawnQueueIsEmpty = true;
        }

        void Start()
        {
            targetFrameDuration = 1f / Engine.TargetFPS;

            Chunks = new Dictionary<string, Chunk>();
            ChunkUpdateQueue = new List<Chunk>();
            frameStopwatch = new Stopwatch();

            InitChunkObject();

            _isDone = true;
            SpawningChunks = false;

            Initialized = true;
        }

        void InitChunkObject()
        {
            Chunk ch = ChunkObject.GetComponent<Chunk>();
            Vector3 s = ChunkObject.transform.localScale;

            ch.meshContainer.transform.localScale = s; // set correct scale of trigger collider and additional mesh collider
            ch.chunkCollider.transform.localScale = s;
            Engine.ChunkScale = s;
        }


        void ResetFrameStopwatch()
        {
            frameStopwatch.Stop();
            frameStopwatch.Reset();
            frameStopwatch.Start();
        }


        public static void AddChunkToUpdateQueue(Chunk chunk)
        {
            if (ChunkUpdateQueue.Contains(chunk) == false)
            {
                ChunkUpdateQueue.Add(chunk);
            }
        }

        void ProcessChunkQueue()
        { // called from the SpawnChunks loop to update chunk meshes
            // update the first chunk and remove it from the queue
            Chunk currentChunk = ChunkUpdateQueue[0];

            if (!currentChunk.empty && !currentChunk.disableMesh)
            {
                currentChunk.RebuildMesh();
            }
            currentChunk.fresh = false;
            ChunkUpdateQueue.RemoveAt(0);
        }

        bool ProcessChunkQueueLoopActive;
        IEnumerator ProcessChunkQueueLoop()
        { // called from Update when SpawnChunks is not running
            ProcessChunkQueueLoopActive = true;
            while (ChunkUpdateQueue.Count > 0 && !SpawningChunks && !StopSpawning)
            {
                ProcessChunkQueue();
                if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            ProcessChunkQueueLoopActive = false;
        }


        public static void RegisterChunk(Chunk chunk)
        { // adds a reference to the chunk to the global chunk list
            Chunks.Add(chunk.chunkIndex.ToString(), chunk);
        }
        public static void UnregisterChunk(Chunk chunk)
        {
            Chunks.Remove(chunk.chunkIndex.ToString());
        }

        public static GameObject GetChunk(int x, int y, int z)
        {
            return GetChunk(new Index(x, y, z));
        }
        public static GameObject GetChunk(Index index)
        {
            Chunk ch = GetChunkComponent(index);
            return (ch == null) ? null : ch.gameObject;
        }

        public static Chunk GetChunkComponent(int x, int y, int z)
        {
            return GetChunkComponent(new Index(x, y, z));
        }
        public static Chunk GetChunkComponent(Index index)
        {
            string s = index.ToString();
            return Chunks.ContainsKey(s) ? Chunks[s] : null;
        }


        #region SpawnChunks

        public static GameObject SpawnChunk(int x, int y, int z)
        { 
            // spawns a single chunk (only if it's not already spawned)
            GameObject chunk = GetChunk(x, y, z);
            if (chunk == null)
            {
                return Engine.ChunkManagerInstance.DoSpawnChunk(new Index(x, y, z));
            }
            else return chunk;
        }
        public static GameObject SpawnChunk(Index index)
        { 
            // spawns a single chunk (only if it's not already spawned)
            GameObject ch = GetChunk(index);
            return (ch == null) ? Engine.ChunkManagerInstance.DoSpawnChunk(index) : ch;
        }

        public static GameObject SpawnChunkFromServer(Index index)
        {
            // spawns a chunk and disables mesh generation and enables timeout (used by the server in multiplayer)
            GameObject g = GetChunk(index);
            if (g == null)
            {
                g = Engine.ChunkManagerInstance.DoSpawnChunk(index);
                Chunk ch = g.GetComponent<Chunk>();
                ch.enableTimeout = true;
                ch.disableMesh = true;
            }
            return g;
        }
        public static GameObject SpawnChunkFromServer(int x, int y, int z)
        {
            return SpawnChunkFromServer(new Index(x, y, z));
        }
        
        GameObject DoSpawnChunk(Index index)
        {
            GameObject g = InstantiateChunk(index.ToVector3(), transform.rotation);
            Chunk ch = g.GetComponent<Chunk>();
            AddChunkToUpdateQueue(ch);

            return g;
        }


        public static void SpawnChunks(float x, float y, float z)
        { 
            Index index = Engine.PositionToChunkIndex(new Vector3(x, y, z));
            Engine.ChunkManagerInstance.TrySpawnChunks(index);
        }
        public static void SpawnChunks(Vector3 position)
        {
            Index index = Engine.PositionToChunkIndex(position);
            Engine.ChunkManagerInstance.TrySpawnChunks(index);
        }

        public static void SpawnChunks(int x, int y, int z)
        {
            Engine.ChunkManagerInstance.TrySpawnChunks(x, y, z);
        }
        public static void SpawnChunks(Index index)
        {
            Engine.ChunkManagerInstance.TrySpawnChunks(index.x, index.y, index.z);
        }


        void TrySpawnChunks(Index index)
        {
            TrySpawnChunks(index.x, index.y, index.z);
        }
        void TrySpawnChunks(int x, int y, int z)
        {
            if (_isDone)
            { 
                // if we're not spawning chunks at the moment, just spawn them normally
                StartSpawnChunks(x, y, z);
            }
            else
            { 
                // if we are spawning chunks already, flag to spawn again once the previous round is finished using the last requested position as origin.
                LastRequest = new Index(x, y, z);
                SpawnQueueIsEmpty = false;
                StopSpawning = true;
                ChunkUpdateQueue.Clear();
            }
        }

        GameObject InstantiateChunk(Vector3 position, Quaternion rotation)
        {
            GameObject ch = Instantiate(ChunkObject, position, rotation) as GameObject;
            ch.transform.SetParent(_chunkContainer);

            return ch;
        }

        #endregion SpawnChunks


        public void Update()
        {
            if (_isDone && !SpawnQueueIsEmpty)
            {
                // if there is a chunk spawn queued up, and if the previous one has finished, run the queued chunk spawn.
                SpawnQueueIsEmpty = true;
                if (LastRequest != null)
                {
                    StartSpawnChunks(LastRequest.x, LastRequest.y, LastRequest.z);
                }
            }

            // if not currently spawning chunks, process any queued chunks here instead
            if (!SpawningChunks && !ProcessChunkQueueLoopActive && ChunkUpdateQueue != null && ChunkUpdateQueue.Count > 0)
            {
                StartCoroutine(ProcessChunkQueueLoop());
            }

            ResetFrameStopwatch();
        }


        void StartSpawnChunks(int originX, int originY, int originZ)
        {
            SpawningChunks = true;
            _isDone = false;

            int range = Engine.ChunkSpawnDistance;

            StartCoroutine(SpawnMissingChunks(originX, originY, originZ, range));
        }


        IEnumerator SpawnMissingChunks(int originX, int originY, int originZ, int range)
        {
            int heightRange = Engine.HeightRange;

            ChunkUpdateQueue = new List<Chunk>(); // clear update queue - it will be repopulated again in the correct order in the following loop

            // flag chunks not in range for removal
            ChunksToDestroy = new List<Chunk>();
            foreach (Chunk chunk in Chunks.Values)
            {
                if (Vector2.Distance(new Vector2(chunk.chunkIndex.x, chunk.chunkIndex.z), new Vector2(originX, originZ)) > range + Engine.ChunkDespawnDistance)
                {
                    ChunksToDestroy.Add(chunk);
                }
                else if (Mathf.Abs(chunk.chunkIndex.y - originY) > range + Engine.ChunkDespawnDistance)
                { 
                    // destroy chunks outside of vertical range
                    ChunksToDestroy.Add(chunk);
                }
            }


            // main loop
            for (int currentLoop = 0; currentLoop <= range; currentLoop++)
            {
                for (var x = originX - currentLoop; x <= originX + currentLoop; x++)
                {
                    // iterate through all potential chunk indexes within range
                    for (var y = originY - currentLoop; y <= originY + currentLoop; y++)
                    {
                        for (var z = originZ - currentLoop; z <= originZ + currentLoop; z++)
                        {
                            if (Mathf.Abs(y) <= heightRange)
                            { 
                                // skip chunks outside of height range
                                if (Mathf.Abs(originX - x) + Mathf.Abs(originZ - z) < range * 1.3f)
                                { 
                                    // skip corners
                                    // pause loop while the queue is not empty
                                    while (ChunkUpdateQueue.Count > 0)
                                    {
                                        ProcessChunkQueue();
                                        if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                        {
                                            yield return new WaitForEndOfFrame();
                                        }
                                    }

                                    Chunk currentChunk = GetChunkComponent(x, y, z);


                                    // chunks that already exist but haven't had their mesh built yet should be added to the update queue
                                    if (currentChunk != null)
                                    {
                                        // chunks without meshes spawned by server should be changed to regular chunks
                                        if (currentChunk.disableMesh || currentChunk.enableTimeout)
                                        {
                                            currentChunk.disableMesh = false;
                                            currentChunk.enableTimeout = false;
                                            currentChunk.fresh = true;
                                        }

                                        if (currentChunk.fresh)
                                        {
                                            // spawn neighbor chunks
                                            for (int d = 0; d < 6; d++)
                                            {
                                                Index neighborIndex = currentChunk.chunkIndex.GetAdjacentIndex((Direction)d);
                                                GameObject neighborChunk = GetChunk(neighborIndex);
                                                if (neighborChunk == null)
                                                {
                                                    neighborChunk = InstantiateChunk(neighborIndex.ToVector3(), transform.rotation);
                                                }
                                                currentChunk.neighborChunks[d] = neighborChunk.GetComponent<Chunk>(); // always add the neighbor to NeighborChunks, in case it's not there already

                                                // continue loop in next frame if the current frame time is exceeded
                                                if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                                {
                                                    yield return new WaitForEndOfFrame();
                                                }
                                                if (StopSpawning)
                                                {
                                                    EndSequence();
                                                    yield break;
                                                }
                                            }

                                            if (currentChunk != null)
                                            {
                                                currentChunk.AddToQueueWhenReady();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // if chunk doesn't exist, create new chunk (it adds itself to the update queue when its data is ready)
                                        // spawn chunk
                                        GameObject newChunk = InstantiateChunk(new Vector3(x, y, z), transform.rotation);
                                        currentChunk = newChunk.GetComponent<Chunk>();

                                        // spawn neighbor chunks if they're not spawned yet
                                        for (int d = 0; d < 6; d++)
                                        {
                                            Index neighborIndex = currentChunk.chunkIndex.GetAdjacentIndex((Direction)d);
                                            GameObject neighborChunk = GetChunk(neighborIndex);
                                            if (neighborChunk == null)
                                            {
                                                neighborChunk = InstantiateChunk(neighborIndex.ToVector3(), transform.rotation);
                                            }
                                            currentChunk.neighborChunks[d] = neighborChunk.GetComponent<Chunk>(); // always add the neighbor to NeighborChunks, in case it's not there already

                                            // continue loop in next frame if the current frame time is exceeded
                                            if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                            {
                                                yield return new WaitForEndOfFrame();
                                            }
                                            if (StopSpawning)
                                            {
                                                EndSequence();
                                                yield break;
                                            }
                                        }

                                        if (currentChunk != null)
                                        {
                                            currentChunk.AddToQueueWhenReady();
                                        }
                                    }
                                }
                            }


                            // continue loop in next frame if the current frame time is exceeded
                            if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                            {
                                yield return new WaitForEndOfFrame();
                            }
                            if (StopSpawning)
                            {
                                EndSequence();
                                yield break;
                            }
                        }
                    }
                }
            }

            yield return new WaitForEndOfFrame();
            EndSequence();
        }


        void EndSequence()
        {
            SpawningChunks = false;
            Resources.UnloadUnusedAssets();
            _isDone = true;
            StopSpawning = false;

            foreach (Chunk chunk in ChunksToDestroy)
            {
                chunk.FlagToRemove();
            }
        }
    }
}