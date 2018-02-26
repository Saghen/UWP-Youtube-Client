using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTApp.Classes.DataTypes
{
    public class CommentDataType
    {
        string authorThumbnail;
        string author;
        string content;
        int replyCount;
        bool isReplies;
        string datePosted;
        long? likeCount;
        string id;
        List<CommentDataType> replies = new List<CommentDataType>();

        public string AuthorThumbnail { get => authorThumbnail; set => authorThumbnail = value; }
        public string Author { get => author; set => author = value; }
        public string Content { get => content; set => content = value; }
        public int ReplyCount { get => replyCount; set => replyCount = value; }
        public bool IsReplies { get => isReplies; set => isReplies = value; }
        public string DatePosted { get => datePosted; set => datePosted = value; }
        public long? LikeCount { get => likeCount; set => likeCount = value; }
        public string Id { get => id; set => id = value; }
        public List<CommentDataType> Replies { get => replies; set => replies = value; }
    }
}
