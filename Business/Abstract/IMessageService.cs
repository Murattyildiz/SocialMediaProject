using Core.Utilities.Result.Abstract;
using Entities.Concrete;
using Entities.DTOs;

namespace Business.Abstract
{
    public interface IMessageService
    {
        IDataResult<Message> GetById(int messageId);
        IDataResult<List<Message>> GetAllBySenderId(int senderId);
        IDataResult<List<Message>> GetAllByReceiverId(int receiverId);
        IDataResult<List<Message>> GetConversation(int user1Id, int user2Id);
        IResult Add(Message message);
        IResult Update(Message message);
        IResult Delete(Message message);
        IResult MarkAsRead(int messageId);
    }
} 