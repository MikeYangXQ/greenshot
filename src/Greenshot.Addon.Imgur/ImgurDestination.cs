﻿#region Greenshot GNU General Public License

// Greenshot - a free and open source screenshot tool
// Copyright (C) 2007-2018 Thomas Braun, Jens Klingen, Robin Krom
// 
// For more information see: http://getgreenshot.org/
// The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 1 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Usings

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapplo.Addons.Bootstrapper.Resolving;
using Dapplo.Log;
using Dapplo.Windows.Extensions;
using Greenshot.Addon.Imgur.Entities;
using Greenshot.Addon.Imgur.ViewModels;
using Greenshot.Addons.Addons;
using Greenshot.Addons.Controls;
using Greenshot.Addons.Core;
using Greenshot.Addons.Interfaces;
using Greenshot.Gfx;

#endregion

namespace Greenshot.Addon.Imgur
{
    /// <summary>
    ///     Description of ImgurDestination.
    /// </summary>
    [Destination("Imgur")]
    [Export]
    public class ImgurDestination : AbstractDestination
	{
	    private static readonly LogSource Log = new LogSource();
        private readonly IImgurConfiguration _imgurConfiguration;
	    private readonly IImgurLanguage _imgurLanguage;
	    private readonly ImgurApi _imgurApi;
	    private readonly ImgurHistoryViewModel _imgurHistoryViewModel;

	    [ImportingConstructor]
		public ImgurDestination(IImgurConfiguration imgurConfiguration, IImgurLanguage imgurLanguage, ImgurApi imgurApi, ImgurHistoryViewModel imgurHistoryViewModel)
		{
			_imgurConfiguration = imgurConfiguration;
		    _imgurLanguage = imgurLanguage;
		    _imgurApi = imgurApi;
		    _imgurHistoryViewModel = imgurHistoryViewModel;
		}

		public override string Description => _imgurLanguage.UploadMenuItem;

		public override Bitmap DisplayIcon
		{
			get
			{
			    // TODO: Optimize this
			    var embeddedResource = GetType().Assembly.FindEmbeddedResources(@".*Imgur\.png").FirstOrDefault();
			    using (var bitmapStream = GetType().Assembly.GetEmbeddedResourceAsStream(embeddedResource))
			    {
			        return BitmapHelper.FromStream(bitmapStream);
			    }
            }
		}

		public override async Task<ExportInformation> ExportCaptureAsync(bool manuallyInitiated, ISurface surface, ICaptureDetails captureDetails)
		{
		    var uploadUrl = await Upload(captureDetails, surface).ConfigureAwait(true);

            var exportInformation = new ExportInformation(Designation, Description)
		    {
		        ExportMade = uploadUrl != null,
		        Uri = uploadUrl?.AbsoluteUri
		    };
		    ProcessExport(exportInformation, surface);
			return exportInformation;
		}

        /// <summary>
        /// Upload the capture to imgur
        /// </summary>
        /// <param name="captureDetails">ICaptureDetails</param>
        /// <param name="surfaceToUpload">ISurface</param>
        /// <returns>Uri</returns>
        private async Task<Uri> Upload(ICaptureDetails captureDetails, ISurface surfaceToUpload)
        {
            try
            {
                ImgurImage imgurImage;

                var cancellationTokenSource = new CancellationTokenSource();
                // TODO: Replace the form
                using (var pleaseWaitForm = new PleaseWaitForm("Imgur", _imgurLanguage.CommunicationWait, cancellationTokenSource))
                {
                    pleaseWaitForm.Show();
                    try
                    {
                        imgurImage = await _imgurApi.UploadToImgurAsync(surfaceToUpload, captureDetails.Title, null, cancellationTokenSource.Token).ConfigureAwait(true);
                        if (imgurImage != null)
                        {
                            // Create thumbnail
                            using (var tmpImage = surfaceToUpload.GetBitmapForExport())
                            using (var thumbnail = tmpImage.CreateThumbnail(90, 90))
                            {
                                imgurImage.Image = thumbnail.ToBitmapSource();
                            }
                            if (_imgurConfiguration.AnonymousAccess && _imgurConfiguration.TrackHistory)
                            {
                                Log.Debug().WriteLine("Storing imgur upload for hash {0} and delete hash {1}", imgurImage.Data.Id, imgurImage.Data.Deletehash);
                                _imgurConfiguration.ImgurUploadHistory.Add(imgurImage.Data.Id, imgurImage.Data.Deletehash);
                                _imgurConfiguration.RuntimeImgurHistory.Add(imgurImage.Data.Id, imgurImage);

                                // Update history
                                _imgurHistoryViewModel.ImgurHistory.Add(imgurImage);
                            }
                        }
                    }
                    finally
                    {
                        pleaseWaitForm.Close();
                    }
                }

                if (imgurImage != null)
                {
                    var uploadUrl = _imgurConfiguration.UsePageLink ? imgurImage.Data.LinkPage: imgurImage.Data.Link;
                    if (uploadUrl == null || !_imgurConfiguration.CopyLinkToClipboard)
                    {
                        return uploadUrl;
                    }

                    try
                    {
                        ClipboardHelper.SetClipboardData(uploadUrl.AbsoluteUri);

                    }
                    catch (Exception ex)
                    {
                        Log.Error().WriteLine(ex, "Can't write to clipboard: ");
                        uploadUrl = null;
                    }
                    return uploadUrl;
                }
            }
            catch (Exception e)
            {
                Log.Error().WriteLine(e, "Error uploading.");
                MessageBox.Show(_imgurLanguage.UploadFailure + " " + e.Message);
            }
            return null;
        }
    }
}