﻿//
// Copyright  Microsoft Corporation ("Microsoft").
//
// Microsoft grants you the right to use this software in accordance with your subscription agreement, if any, to use software 
// provided for use with Microsoft Azure ("Subscription Agreement").  All software is licensed, not sold.  
// 
// If you do not have a Subscription Agreement, or at your option if you so choose, Microsoft grants you a nonexclusive, perpetual, 
// royalty-free right to use and modify this software solely for your internal business purposes in connection with Microsoft Azure 
// and other Microsoft products, including but not limited to, Microsoft R Open, Microsoft R Server, and Microsoft SQL Server.  
// 
// Unless otherwise stated in your Subscription Agreement, the following applies.  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT 
// WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL MICROSOFT OR ITS LICENSORS BE LIABLE 
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
// TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE SAMPLE CODE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//

using System.Collections.Generic;

namespace ImageClassifier.Interfaces
{
    /// <summary>
    /// Base for the event when the labels of the multi image data source are changed.
    /// </summary>
    /// <param name="containerLabels"></param>
    public delegate void OnContainerLabelsAcquired(IEnumerable<string> containerLabels);

    /// <summary>
    /// Extends the IDataSource for multi image sources. Multi image sources use path information
    /// of the container to infer initial classifications for the objects in the collection.
    /// </summary>
    public interface IMultiImageDataSource : IDataSource
    {
        /// <summary>
        /// Notifiy parent controls that new labels have been acquired from the containers.
        /// </summary>
        event OnContainerLabelsAcquired OnLabelsAcquired;
        /// <summary>
        /// Gets the current container as a classificaiton entry
        /// </summary>
        string CurrentContainerAsClassification { get; }
        /// <summary>
        /// Collect the container labels
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetContainerLabels();
        /// <summary>
        /// Gets the current batch size
        /// </summary>
        int BatchSize { get; }
        /// <summary>
        /// Request the next item in the current container
        /// collection.
        /// </summary>
        /// <returns>SourceFile indicating information for the next item</returns>
        IEnumerable<SourceFile> NextSourceGroup();
        /// <summary>
        /// Request the previous item in the current container
        /// collection.
        /// </summary>
        /// <returns>SourceFile indicating information for the previous item</returns>
        IEnumerable<SourceFile> PreviousSourceGroup();
        /// <summary>
        /// Update a batch of files in the IDataSink to expedite disk writes.
        /// </summary>
        void UpdateSourceBatch(IEnumerable<SourceFile> fileBatch);
    }
}
