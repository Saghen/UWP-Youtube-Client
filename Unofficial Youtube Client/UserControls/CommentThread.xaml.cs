using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YTApp.Classes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace YTApp.UserControls
{
    public sealed partial class CommentThread : UserControl
    {
        bool isExpanded = false;

        private ObservableCollection<Classes.DataTypes.CommentDataType> commentReplies = new ObservableCollection<Classes.DataTypes.CommentDataType>();

        private static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(Classes.DataTypes.CommentDataType), typeof(CommentThread), null);

        public Classes.DataTypes.CommentDataType Source
        {
            get { return (Classes.DataTypes.CommentDataType)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); if (Source.ReplyCount == 0) { Replies.Visibility = Visibility.Collapsed; } }
        }

        public CommentThread()
        {
            Loaded += CommentThread_Loaded;
            this.InitializeComponent();
        }

        private void CommentThread_Loaded(object sender, RoutedEventArgs e)
        {
            //Load replies
            foreach (var item in Source.Replies)
            {
                if (commentReplies.Count > 2)
                    break;
                if (item.LikeCount == 0)
                    item.LikeCount = null;
                commentReplies.Add(item);
            }  
        }

        private void ReplyButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ReplyContainer.Visibility == Visibility.Collapsed)
                ReplyContainer.Visibility = Visibility.Visible;
            else
                ReplyContainer.Visibility = Visibility.Collapsed;
        }


        private async void ViewMoreButton_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            if (!isExpanded)
            {
                isExpanded = !isExpanded;

                //ViewMoreButton.

                var methods = new YoutubeMethods();

                var service = YoutubeMethodsStatic.GetServiceNoAuth();
                var getReplies = service.Comments.List("snippet");
                getReplies.ParentId = Source.Id;
                getReplies.TextFormat = Google.Apis.YouTube.v3.CommentsResource.ListRequest.TextFormatEnum.PlainText;
                var response = await getReplies.ExecuteAsync();

                commentReplies.Clear();

                foreach (var item in response.Items)
                {
                    var comment = methods.CommentToDataType(item);

                    //If there are no likes, set it to null
                    if (comment.LikeCount == 0)
                        comment.LikeCount = null;
                    commentReplies.Add(comment);
                }
                    
            }
            else
            {
                isExpanded = !isExpanded;

                commentReplies.Clear();

                foreach (var item in Source.Replies)
                {
                    if (commentReplies.Count > 2)
                        break;

                    //If there are no likes, set it to null
                    if (item.LikeCount == 0)
                        item.LikeCount = null;
                    commentReplies.Add(item);
                }
            }
        }

        private async void ReplyBoxSend_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            if (ReplyBox.Text == null || ReplyBox.Text == "")
            {
                ReplyContainer.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                var service = await YoutubeMethodsStatic.GetServiceAsync();

                Comment comment = new Comment();
                comment.Snippet = new CommentSnippet();
                comment.Snippet.TextOriginal = ReplyBox.Text;
                comment.Snippet.ParentId = Source.Id;

                var returnedComment = await service.Comments.Insert(comment, "snippet").ExecuteAsync();

                //Add the comment to the UI
                var methods = new YoutubeMethods();
                commentReplies.Add(methods.CommentToDataType(returnedComment));

                //Hide the reply box
                ReplyContainer.Visibility = Visibility.Collapsed;
            }
            catch(Google.GoogleApiException ex)
            {
                if (ex.Error.Code == 403)
                    Constants.MainPageRef.ShowNotifcation("You have not setup a channel with your account. Please do so to post a comment.", 0);
                else if (ex.Error.Code == 400)
                    Constants.MainPageRef.ShowNotifcation("Your comment was too long or Youtube failed to handle the request correctly.", 5000);
                else
                    Constants.MainPageRef.ShowNotifcation("An error occured.");
            }
        }
    }
}
