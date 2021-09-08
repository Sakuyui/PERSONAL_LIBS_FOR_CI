using System;
using System.Collections.Generic;

namespace CIExam.Structure
{
    public class BinaryTreeNode<T>
    {
        public T Data;
        public BinaryTreeNode<T> Left;
        public BinaryTreeNode<T> Right;

        public IEnumerable<BinaryTreeNode<T>> PreorderEnumerator => GetPreorderEnumerator();
        
        public IEnumerable<BinaryTreeNode<T>> InorderEnumerator => GetInorderEnumerator();
        public IEnumerable<BinaryTreeNode<T>> PosterOrderEnumerator => GetPosterOrderEnumerator();

        private IEnumerable<BinaryTreeNode<T>> GetPreorderEnumerator()
        {
            var ans = new List<BinaryTreeNode<T>>();
            PreOrderDfs((data, objects) =>
            {
                ((List<BinaryTreeNode<T>>)objects[0]).Add(data);
                return null;
            }, ans);
            return ans;
        }
        private IEnumerable<BinaryTreeNode<T>> GetInorderEnumerator()
        {
            var ans = new List<BinaryTreeNode<T>>();
            InOrderDfs((data, objects) =>
            {
                ((List<BinaryTreeNode<T>>)objects[0]).Add(data);
                return null;
            }, ans);
            return ans;
        }
        private IEnumerable<BinaryTreeNode<T>> GetPosterOrderEnumerator()
        {
            var ans = new List<BinaryTreeNode<T>>();
            PosterOrderDfs((data, objects) =>
            {
                ((List<BinaryTreeNode<T>>)objects[0]).Add(data);
                return null;
            }, ans);
            return ans;
        }

        
        public BinaryTreeNode(T value)
        {
            Data = value;
        }

        public bool IsLeaf()
        {
            return Left == null && Right == null;
        }
        
        public delegate object VisitStrategy(BinaryTreeNode<T> data, params object[] objects);

        public object PreOrderDfs(VisitStrategy visitStrategy, params object[] objects)
        {
            visitStrategy(this, objects);
            Left?.PreOrderDfs(visitStrategy, objects);
            Right?.PreOrderDfs(visitStrategy, objects);
            return null;
        } 
        
        public object InOrderDfs(VisitStrategy visitStrategy, params object[] objects)
        {
            Left?.InOrderDfs(visitStrategy, objects);
            visitStrategy(this, objects);
            Right?.InOrderDfs(visitStrategy, objects);
            return null;
        } 
        public object PosterOrderDfs(VisitStrategy visitStrategy, params object[] objects)
        {
           
            Left?.PosterOrderDfs(visitStrategy, objects);
            Right?.PosterOrderDfs(visitStrategy, objects);
            visitStrategy(this, objects);
            return null;
        } 

    }
}