﻿//using NUnit.Framework;
//using openHistorian.Collections.Generic.TreeNodes;

//namespace openHistorian.Collections.Generic
//{
//    [TestFixture]
//    public class ZeroLeafNodeTest
//    {
//        private const int Max = 1000000;

//        [Test]
//        public void TestSequently()
//        {
//            TreeNodeBase<TreeUInt32, TreeUInt32> tree = TreeNodeInitializer.CreateTreeNode<TreeUInt32, TreeUInt32>(SortedTree.ZeroNode, 0);

//            LeafNodeTest.TestNode(tree, new SequentialTest(), 5000);
//        }

//        [Test]
//        public void TestReverseSequently()
//        {
//            TreeNodeBase<TreeUInt32, TreeUInt32> tree = TreeNodeInitializer.CreateTreeNode<TreeUInt32, TreeUInt32>(SortedTree.ZeroNode, 0);

//            LeafNodeTest.TestNode(tree, new ReverseSequentialTest(), 5000);
//        }

//        [Test]
//        public void TestRandom()
//        {
//            TreeNodeBase<TreeUInt32, TreeUInt32> tree = TreeNodeInitializer.CreateTreeNode<TreeUInt32, TreeUInt32>(SortedTree.ZeroNode, 0);

//            LeafNodeTest.TestNode(tree, new RandomTest(), 2000);
//        }

//        [Test]
//        public void BenchmarkSequently()
//        {
//            TreeNodeInitializer<TreeUInt32, TreeUInt32> tree = TreeNodeInitializer.GetTreeNodeInitializer<TreeUInt32, TreeUInt32>(SortedTree.ZeroNode);

//            LeafNodeTest.TestSpeed(tree, new SequentialTest(), 500, 512);
//        }

//        [Test]
//        public void BenchmarkReverseSequently()
//        {
//            TreeNodeInitializer<TreeUInt32, TreeUInt32> tree = TreeNodeInitializer.GetTreeNodeInitializer<TreeUInt32, TreeUInt32>(SortedTree.ZeroNode);

//            LeafNodeTest.TestSpeed(tree, new ReverseSequentialTest(), 500, 512);
//        }

//        [Test]
//        public void BenchmarkRandom()
//        {
//            TreeNodeInitializer<TreeUInt32, TreeUInt32> tree = TreeNodeInitializer.GetTreeNodeInitializer<TreeUInt32, TreeUInt32>(SortedTree.ZeroNode);

//            LeafNodeTest.TestSpeed(tree, new RandomTest(), 500, 512);
//        }

//        //        [Test]
//        //        public void TestReverseSequently()
//        //        {

//        //            uint rootKey = 0;
//        //            byte rootLevel = 0;

//        //            uint nextKeyIndex = 0;
//        //            bool hasChanged = false;
//        //            Func<uint> getNextKey = () =>
//        //            {
//        //                nextKeyIndex++;
//        //                return nextKeyIndex;
//        //            };

//        //            Stopwatch swWrite = new Stopwatch();
//        //            Stopwatch swRead = new Stopwatch();
//        //            using (var bs = new BinaryStream())
//        //            {
//        //                bs.Write(0);
//        //                SparseIndexBase<KeyValue256> ln = new SortedTree<KeyValue256, KeyValue256Methods, SortedTree256CompressionNone>.SparseIndex(bs, 125 * 10, getNextKey);
//        //                var k = new KeyValue256();
//        //                ln.Initialize(0, 0);
//        //                long index;

//        //                swWrite.Start();
//        //                for (uint x = 0; x < Max; x++)
//        //                {
//        //#if !SkipAssert
//        //                    hasChanged = !(rootKey == ln.RootNodeIndexAddress && rootLevel == ln.RootNodeLevel);
//        //                    Assert.AreEqual(hasChanged, ln.HasChanged);
//        //                    if (hasChanged)
//        //                    {
//        //                        ln.Save(out rootLevel, out rootKey);
//        //                        hasChanged = false;
//        //                        Assert.AreEqual(hasChanged, ln.HasChanged);
//        //                    }

//        //#endif
//        //                    k.Key1 = (2 * Max) - x;
//        //                    ln.Add(k, x + 1);
//        //                }
//        //                swWrite.Stop();

//        //                swRead.Start();
//        //                for (uint x = 0; x < Max; x++)
//        //                {
//        //                    k.Key1 = (2 * Max) - x;
//        //                    index = ln.Get(k);
//        //#if !SkipAssert
//        //                    Assert.AreEqual((long)x + 1, index);
//        //#endif
//        //                }
//        //                swRead.Stop();

//        //                ln = ln;
//        //            }

//        //            Console.WriteLine((Max / swRead.Elapsed.TotalSeconds / 1000000).ToString("0.000 MPS Read"));
//        //            Console.WriteLine((Max / swWrite.Elapsed.TotalSeconds / 1000000).ToString("0.000 MPS Write"));
//        //        }

//        //        [Test]
//        //        public void TestRandom()
//        //        {
//        //            uint rootKey = 0;
//        //            byte rootLevel = 0;

//        //            uint nextKeyIndex = 0;
//        //            bool hasChanged = false;
//        //            Func<uint> getNextKey = () =>
//        //            {
//        //                nextKeyIndex++;
//        //                return nextKeyIndex;
//        //            };

//        //            Stopwatch swWrite = new Stopwatch();
//        //            Stopwatch swRead = new Stopwatch();
//        //            using (var bs = new BinaryStream())
//        //            {
//        //                bs.Write(0);
//        //                SparseIndexBase<KeyValue256> ln = new SortedTree<KeyValue256, KeyValue256Methods, SortedTree256CompressionNone>.SparseIndex(bs, 125 * 10, getNextKey);
//        //                var k = new KeyValue256();
//        //                ln.Initialize(0, 0);
//        //                long index;

//        //                swWrite.Start();
//        //                for (uint x = 0; x < Max; x++)
//        //                {
//        //#if !SkipAssert
//        //                    hasChanged = !(rootKey == ln.RootNodeIndexAddress && rootLevel == ln.RootNodeLevel);
//        //                    Assert.AreEqual(hasChanged, ln.HasChanged);
//        //                    if (hasChanged)
//        //                    {
//        //                        ln.Save(out rootLevel, out rootKey);
//        //                        hasChanged = false;
//        //                        Assert.AreEqual(hasChanged, ln.HasChanged);
//        //                    }

//        //#endif
//        //                    if ((x & 1) == 0)
//        //                        k.Key1 = (2 * Max) - x;
//        //                    else
//        //                        k.Key1 = (2 * Max) + x;
//        //                    ln.Add(k, x + 1);
//        //                }
//        //                swWrite.Stop();

//        //                swRead.Start();
//        //                for (uint x = 0; x < Max; x++)
//        //                {
//        //                    if ((x & 1) == 0)
//        //                        k.Key1 = (2 * Max) - x;
//        //                    else
//        //                        k.Key1 = (2 * Max) + x;
//        //                    index = ln.Get(k);
//        //#if !SkipAssert
//        //                    Assert.AreEqual((long)x + 1, index);
//        //#endif
//        //                }
//        //                swRead.Stop();

//        //                ln = ln;
//        //            }

//        //            Console.WriteLine((Max / swRead.Elapsed.TotalSeconds / 1000000).ToString("0.000 MPS Read"));
//        //            Console.WriteLine((Max / swWrite.Elapsed.TotalSeconds / 1000000).ToString("0.000 MPS Write"));
//        //        }


//        //        [Test]
//        //        public void TestTrueRandom()
//        //        {
//        //            int seed = 6;
//        //            Random r = new Random(seed);
//        //            uint rootKey = 0;
//        //            byte rootLevel = 0;

//        //            uint nextKeyIndex = 0;
//        //            bool hasChanged = false;
//        //            Func<uint> getNextKey = () =>
//        //            {
//        //                nextKeyIndex++;
//        //                return nextKeyIndex;
//        //            };

//        //            Stopwatch swWrite = new Stopwatch();
//        //            Stopwatch swRead = new Stopwatch();
//        //            using (var bs = new BinaryStream())
//        //            {
//        //                bs.Write(0);
//        //                SparseIndexBase<KeyValue256> ln = new SortedTree<KeyValue256, KeyValue256Methods, SortedTree256CompressionNone>.SparseIndex(bs, 125 * 10, getNextKey);
//        //                var k = new KeyValue256();
//        //                ln.Initialize(0, 0);
//        //                long index;

//        //                swWrite.Start();
//        //                for (uint x = 0; x < Max; x++)
//        //                {
//        //#if !SkipAssert
//        //                    hasChanged = !(rootKey == ln.RootNodeIndexAddress && rootLevel == ln.RootNodeLevel);
//        //                    Assert.AreEqual(hasChanged, ln.HasChanged);
//        //                    if (hasChanged)
//        //                    {
//        //                        ln.Save(out rootLevel, out rootKey);
//        //                        hasChanged = false;
//        //                        Assert.AreEqual(hasChanged, ln.HasChanged);
//        //                    }
//        //#endif

//        //                    k.Key1 = ((ulong)r.Next()) << 32 | (ulong)r.Next();
//        //                    k.Key2 = ((ulong)r.Next()) << 32 | (ulong)r.Next();
//        //                    ln.Add(k, x + 1);
//        //                }
//        //                swWrite.Stop();

//        //                r = new Random(seed);

//        //                swRead.Start();
//        //                for (uint x = 0; x < Max; x++)
//        //                {
//        //                    k.Key1 = ((ulong)r.Next()) << 32 | (ulong)r.Next();
//        //                    k.Key2 = ((ulong)r.Next()) << 32 | (ulong)r.Next();
//        //                    index = ln.Get(k);
//        //#if !SkipAssert
//        //                    Assert.AreEqual((long)x + 1, index);
//        //#endif
//        //                }
//        //                swRead.Stop();

//        //                ln = ln;
//        //            }

//        //            Console.WriteLine((Max / swRead.Elapsed.TotalSeconds / 1000000).ToString("0.000 MPS Read"));
//        //            Console.WriteLine((Max / swWrite.Elapsed.TotalSeconds / 1000000).ToString("0.000 MPS Write"));
//        //        }
//    }
//}