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

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ImageClassifier.UIUtils;

namespace ImageClassifier.Interfaces.GenericUI
{
    /// <summary>
    /// Interaction logic for MultiImageControl.xaml
    /// </summary>
    public partial class MultiImageControl : UserControl, IMultiImageControl
    {
        private const int MAX_COLUMNS = 3;
        private IMultiImageDataSource MultiImageDataSource { get; set; }

        public MultiImageControl(IDataSource source)
        {
            InitializeComponent();

            this.CurrentSourceBatch = new List<CurrentItem>();
            this.DataSource = source;
            this.MultiImageDataSource = this.DataSource as IMultiImageDataSource;
            this.ButtonNext.Click += (o, e) => NextBatch();
            this.ButtonPrevious.Click += (o, e) => PreviousBatch();

            this.Classifications = new List<string>();
        }

        #region IMultiImageControl
        public event OnImageChanged ImageChanged;

        public List<String> Classifications { get; set; }

        public List<CurrentItem> CurrentSourceBatch { get; private set; }

        public UIElement ParentControl { get; set; }

        public CurrentItem CurrentSourceFile { get; private set; }

        public IDataSource DataSource { get; private set; }

        public UIElement Control { get { return this; } }

        public IEnumerable<KeyBinding> Bindings
        {
            get
            {
                List<KeyBinding> bindings = new List<KeyBinding>();
                bindings.Add(new KeyBinding(
                    new ImageChangeCommand(this.ButtonNext, this.NextBatch),
                    Key.N,
                    ModifierKeys.Control));

                bindings.Add(new KeyBinding(
                    new ImageChangeCommand(this.ButtonPrevious, this.PreviousBatch),
                    Key.P,
                    ModifierKeys.Control));

                return bindings;
            }
        }

        public void Clear()
        {
            this.ImagePanel.Children.Clear();
        }

        public void FastForward()
        {
            this.ShowNext();
        }

        public void ShowNext()
        {
            this.NextBatch();
        }

        public void UpdateClassifications(List<string> classifications)
        {
            // TODO: Does this mke sense for multi image?
            // A check was made on the main window. This is going to be the default
            // for all of them or to be added to all of them.
            int x = 9;

            // 1. Get what the default is from the data source
            // 2. Set all of them to the new settings ONLY unless they have been
            //    manually modified already. 
        }
        #endregion

        #region Navigation

        private void DisplayImages(IEnumerable<SourceFile> files)
        {
            this.Clear();

            // Items are saved as they are modified, so just need to clear out 
            // what we already have and get the next batch.
            this.CurrentSourceBatch.Clear();
            foreach (SourceFile sFile in files)
            {
                this.CurrentSourceBatch.Add(new CurrentItem() { CurrentSource = sFile });
            }

            // Get the size of the parent, default it to 300
            double parentHeight = 300;
            double parentWidth = 300;
            int maxRows = this.CurrentSourceBatch.Count / MultiImageControl.MAX_COLUMNS;
            int maxCols = MultiImageControl.MAX_COLUMNS;

            FrameworkElement fe = this.Parent as FrameworkElement;
            if (fe != null)
            {
                parentHeight = fe.ActualHeight / maxRows;
                parentWidth = fe.ActualWidth / maxCols;
            }


            // Geneerate a grid
            Grid imageGrid = this.BuildGrid(maxRows, maxCols);

            int curRow = 0;
            int curCol = 0;
            foreach (CurrentItem item in this.CurrentSourceBatch)
            {
                MultiImageInstance instance = 
                    new MultiImageInstance(
                        this.MultiImageDataSource, 
                        item, 
                        parentHeight, 
                        parentWidth, 
                        this.Classifications);

                // Place it in the grid
                int thisRow = curRow % maxRows;
                int thisCol = curCol++ % maxCols;
                if ((curCol % maxCols) == 0)
                {
                    curRow++;
                }
                Grid.SetColumn(instance, thisCol);
                Grid.SetRow(instance, thisRow);

                // Add it to the grid
                imageGrid.Children.Add(instance);
            }

            this.ImagePanel.Children.Add(imageGrid);

            
            SourceFile newFile = new SourceFile();
            newFile.Classifications.Add(this.MultiImageDataSource.CurrentContainerAsClassification);
            this.ImageChanged?.Invoke(newFile);                

            // Update the navigation buttons
            this.ButtonNext.IsEnabled = this.MultiImageDataSource.CanMoveNext;
            this.ButtonPrevious.IsEnabled = this.MultiImageDataSource.CanMovePrevious;
        }

        private Grid BuildGrid(int rows, int columns)
        {
            Grid imageGrid = new Grid();

            for(int i=0;i<columns;i++)
            {
                imageGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < columns; i++)
            {
                imageGrid.RowDefinitions.Add(new RowDefinition());
            }
            return imageGrid;
        }

        
        private void NextBatch()
        {
            this.MultiImageDataSource.ClearSourceFiles();
            if (this.MultiImageDataSource != null &&
                this.MultiImageDataSource.CanMoveNext )
            {
                this.DisplayImages(this.MultiImageDataSource.NextSourceGroup());
            }
        }

        private void PreviousBatch()
        {
            this.DataSource.ClearSourceFiles();
            if (this.MultiImageDataSource != null &&
                this.MultiImageDataSource.CanMovePrevious)
            {
                this.DisplayImages(this.MultiImageDataSource.PreviousSourceGroup());
            }
        }
        #endregion
    }
}
