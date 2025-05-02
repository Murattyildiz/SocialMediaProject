using Business.Abstract;
using Business.Constants;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class MessageManager : IMessageService
    {
        private readonly IMessageDal _messageDal;

        public MessageManager(IMessageDal messageDal)
        {
            _messageDal = messageDal;
        }

        public IResult Add(Message message)
        {
            message.SentDate = DateTime.Now;
            message.IsRead = false;
            message.Status = true;
            _messageDal.Add(message);
            return new SuccessResult("Mesaj gönderildi");
        }

        public IResult Delete(Message message)
        {
            _messageDal.Delete(message);
            return new SuccessResult("Mesaj silindi");
        }

        public IDataResult<List<Message>> GetAllByReceiverId(int receiverId)
        {
            return new SuccessDataResult<List<Message>>(_messageDal.GetAll(m => m.ReceiverId == receiverId && m.Status == true));
        }

        public IDataResult<List<Message>> GetAllBySenderId(int senderId)
        {
            return new SuccessDataResult<List<Message>>(_messageDal.GetAll(m => m.SenderId == senderId && m.Status == true));
        }

        public IDataResult<Message> GetById(int messageId)
        {
            return new SuccessDataResult<Message>(_messageDal.Get(m => m.Id == messageId));
        }

        public IDataResult<List<Message>> GetConversation(int user1Id, int user2Id)
        {
            return new SuccessDataResult<List<Message>>(_messageDal.GetConversation(user1Id, user2Id));
        }

        public IResult MarkAsRead(int messageId)
        {
            var message = _messageDal.Get(m => m.Id == messageId);
            if (message == null)
            {
                return new ErrorResult("Mesaj bulunamadı");
            }

            message.IsRead = true;
            _messageDal.Update(message);
            return new SuccessResult("Mesaj okundu olarak işaretlendi");
        }

        public IResult Update(Message message)
        {
            _messageDal.Update(message);
            return new SuccessResult("Mesaj güncellendi");
        }
    }
} 