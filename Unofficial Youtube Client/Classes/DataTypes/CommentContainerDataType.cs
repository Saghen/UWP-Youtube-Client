namespace YTApp.Classes.DataTypes
{
    public class CommentContainerDataType
    {
        //This exists due to the setup we have used on the video page. It is so that we can pass the full object into the source and then 
        public CommentDataType commentData = new CommentDataType();

        public CommentContainerDataType() { }

        public CommentContainerDataType(CommentDataType data) { commentData = data; }
    }
}
