using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using DataAccess.Concrete.Context;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfMessageDal : EfEntityRepositoryBase<Message, SocialMediaContext>, IMessageDal
    {
        public List<Message> GetConversation(int user1Id, int user2Id)
        {
            using (var context = new SocialMediaContext())
            {
                var messages = context.Messages
                    .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) || 
                                (m.SenderId == user2Id && m.ReceiverId == user1Id))
                    .OrderBy(m => m.SentDate)
                    .ToList();
                
                return messages;
            }
        }
    }
} 