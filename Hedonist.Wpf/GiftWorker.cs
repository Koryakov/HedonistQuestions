﻿using Hedonist.Models;
using ModalControl;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media.Imaging;

namespace Hedonist.Wpf {

    public class GiftWorker
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private (AutorizeResultType resultType, GiftQrCodeRawData qrCodeData) giftDataResponse;
        private GiftPageModel giftPageModel;
        private string pageName;

        public GiftWorker(string pageName, GiftPageModel giftPageModel) { 
            this.pageName = pageName;
            this.giftPageModel = giftPageModel;
        }

        public void DoWork() {
            try {
                logger.Debug("IN BgWorker_DoWork()");

                Task getAuthTask = Task.Run(async () => {
                    logger.Debug("BgWorker_DoWork() Task Run()...");
                    giftDataResponse = await ClientEngine.GetGiftAsync(giftPageModel.Ticket, giftPageModel.SelectedAnswers.Last().Id);
                });
                Task.WaitAll(getAuthTask);
                logger.Debug("OUT BgWorker_DoWork; resetEvent.Wait() ended");

            }
            catch (TaskCanceledException ex) {
                giftDataResponse = new(AutorizeResultType.Timeout, null);
                logger.Error(ex, "BgWorker_DoWork() TIMEOUT");
            }
            catch (Exception ex) {
                giftDataResponse = new(AutorizeResultType.Error, null);
                logger.Error(ex, "BgWorker_DoWork() EXCEPTION");
            }
        }

        public bool ProcessObtainedGiftData(out GiftQrCodeCompleteData giftQrCodeCompleteData) {
            giftQrCodeCompleteData = new GiftQrCodeCompleteData();
            try {
                logger.Debug("IN ProcessObtainedGiftData()");

                if (giftDataResponse.resultType == AutorizeResultType.Authorized) {

                    giftQrCodeCompleteData = new GiftQrCodeCompleteData() {
                        RawData = giftDataResponse.qrCodeData
                    };
                    if (giftQrCodeCompleteData.RawData.GiftType.HasQrCode) {
                        giftQrCodeCompleteData.QrCodeImageSource = CreateQrCodeBitmap(giftDataResponse.qrCodeData.QrCodeText);
                    }
                    return true;
                }
                else {
                    logger.Error("ProcessObtainedGiftData() return false");
                    return false;
                }
            } catch(Exception ex) {
                logger.Error(ex, "ProcessObtainedGiftData() EXCEPTION");
                return false;
            }
        }

        private static byte[] CreateQrCodeByteArray(string qrCodeText) {
            var qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeText, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new BitmapByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }

        private BitmapImage CreateQrCodeBitmap(string qrCodeText) {
            byte[] qrCodeByteArray = CreateQrCodeByteArray(qrCodeText);

            //using (var ms = new MemoryStream(giftDataResponse.qrCodeData.QrCodeByteArr)) {
            using (var ms = new MemoryStream(qrCodeByteArray)) {
                    ms.Position = 0;
                BitmapImage imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.StreamSource = ms;
                imageSource.CacheOption = BitmapCacheOption.OnLoad;
                imageSource.EndInit();
                return imageSource;

                //imgQrCode.Source = imageSource;
                //txtQrCode.Text = giftDataResponse.qrCodeData.QrCodeText;
            }
        }
    }

}