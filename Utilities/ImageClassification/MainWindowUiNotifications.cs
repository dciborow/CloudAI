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
using ImageClassifier.Interfaces;
using ImageClassifier.MainWindowUtilities;

namespace ImageClassifier
{
    public partial class MainWindow
    {
        #region Status Bar Events
        /// <summary>
        /// Clears the status of the status bar
        /// </summary>
        private void StatusBarClearStatus()
        {
            this.StatusBarCollection.Text = String.Empty;
            this.StatusBarLocationStatus.Text = String.Empty;
        }

        /// <summary>
        /// Hooked to the jump to button on the status bar to jump the current pointer in
        /// the current IDataSource to another location in it's current collection.
        /// </summary>
        private void StatusBarJumpToImage()
        {
            String error = String.Empty;
            if (this.ApplicationContext.SelectedDataSource != null)
            {
                try
                {
                    int index = int.Parse(this.StatusTextJumpTo.Text);
                    this.ApplicationContext.SelectedDataSource.JumpToSourceFile(index);
                    this.ApplicationContext.SelectedDataSource.ImageControl.ShowNext();
                }
                catch (Exception)
                {
                    error = "Jump to index must be a number";
                }
            }
            else
            {
                error = "A source provider must be present";
            }

            if (!String.IsNullOrEmpty(error))
            {
                System.Windows.MessageBox.Show(error, "Jump To Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Configuration UI Events
        /// <summary>
        /// Hooked to the source provider combo box and triggered whent the selected item is 
        /// changed.
        /// </summary>
        private void SourceProviderChanged()
        {
            if (this.ConfigurationTabSourceProviderCombo.SelectedItem is DataSourceComboItem)
            {
                this.StatusBarClearStatus();

                // Unhook IImageControl callbacks......
                if (this.ApplicationContext.SelectedDataSource != null)
                {
                    this.ApplicationContext.SelectedDataSource.ContainerControl.OnContainerChanged -= this.IContainerControlContainerChanged;

                    if(this.ApplicationContext.SelectedDataSource.ImageControl != null)
                    {
                        if (this.ApplicationContext.IsMultiImageDataSource)
                        {
                            this.ApplicationContext.SelectedDataSource.ImageControl.ImageChanged -= this.IMultiImageControlGroupChanged;
                            (this.ApplicationContext.SelectedDataSource as IMultiImageDataSource).OnLabelsAcquired -= this.MultiImageSourceContainerLabelsChanged;
                        }
                        else
                        {
                            this.ApplicationContext.SelectedDataSource.ImageControl.ImageChanged -= this.ISingleImageControlFileChanged;
                        }
                    }
                }

                this.ApplicationContext.SelectedDataSource = (this.ConfigurationTabSourceProviderCombo.SelectedItem as DataSourceComboItem).Source;

                this.ApplicationContext.AppConfiguration.DefaultProvider = this.ApplicationContext.SelectedDataSource.Name;
                this.ApplicationContext.AppConfiguration.Save();


                // Hook IImageControl callbacks......
                if (this.ApplicationContext.SelectedDataSource != null)
                {
                    if (this.ApplicationContext.SelectedDataSource.ImageControl != null)
                    {
                        if (this.ApplicationContext.IsMultiImageDataSource)
                        {
                            this.ApplicationContext.SelectedDataSource.ImageControl.ImageChanged += this.IMultiImageControlGroupChanged;
                            ((IMultiImageControl)this.ApplicationContext.SelectedDataSource.ImageControl).Classifications = 
                                new List<string>(this.ApplicationContext.SelectedDataSource.Classifications);
                            (this.ApplicationContext.SelectedDataSource as IMultiImageDataSource).OnLabelsAcquired += this.MultiImageSourceContainerLabelsChanged;
                        }
                        else 
                        {
                            this.ApplicationContext.SelectedDataSource.ImageControl.ImageChanged += this.ISingleImageControlFileChanged;
                        }
                    }
                }

                this.ApplicationContext.SelectedDataSource.ContainerControl.OnContainerChanged += this.IContainerControlContainerChanged;
                this.ApplicationContext.SelectedDataSource.ContainerControl.Refresh();

                this.InitializeUi(true);

                // Now set up the configuration
                this.SourceProviderConfigTitle.Text = String.Format("{0} Configuration", this.ApplicationContext.SelectedDataSource.ConfigurationControl.Title);
                this.ProviderConfigurationPanel.Children.Clear();
                this.ProviderConfigurationPanel.Children.Add(this.ApplicationContext.SelectedDataSource.ConfigurationControl.Control);
            }
        }

        /// <summary>
        /// Hooked for the label change event on the IMultiImageControl, but currently not sure this
        /// will be useful or not.
        /// </summary>
        /// <param name="labels"></param>
        private void MultiImageSourceContainerLabelsChanged(IEnumerable<string> labels)
        {
            // TODO: Currently there is nothing to change whent the labels are updated.
        }
        #endregion
    }
}
