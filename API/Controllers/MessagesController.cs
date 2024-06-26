﻿using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            this._userRepository = userRepository;
            this._messageRepository = messageRepository;
            this._mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUserName();
            if (username == createMessageDto.RecipientUserName.ToLower())
            {
                return BadRequest("You cannot send a message to yourself.");
            }

            var sender = await _userRepository.GetUserByUserNameAsync(username);
            var recipient = await _userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName);
            if (recipient == null)
            {
                return NotFound();
            }

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUserName = username,
                RecipientUserName = recipient.Username,
                Content = createMessageDto.Content
            };

            _messageRepository.AddMessage(message);
            if (await _messageRepository.SaveAllAsync())
            {
                return Ok(_mapper.Map<MessageDto>(message));
            }

            return BadRequest("Failed to send message");
        }

        // [HttpGet]
        // public async Task<ActionResult<MessageDto>> GetMessage(int messageId)
        // {
        //     var message = await _messageRepository.GetMessage(messageId);

        //     if (message == null)
        //     {
        //         return BadRequest("Message Id " + messageId + " does not exist.");
        //     }

        //     return Ok(_mapper.Map<MessageDto>(message));

        //     // var sender = await _userRepository.GetUserByUserNameAsync(username);
        //     // var recipient = await _userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName);
        //     // if (recipient == null)
        //     // {
        //     //     return NotFound();
        //     // }

        //     // var message = new Message
        //     // {
        //     //     Sender = sender,
        //     //     Recipient = recipient,
        //     //     SenderUserName = username,
        //     //     RecipientUserName = recipient.Username,
        //     //     Content = createMessageDto.Content
        //     // };

        //     // _messageRepository.AddMessage(message);
        //     // if (await _messageRepository.SaveAllAsync())
        //     // {
        //     //     return Ok(_mapper.Map<MessageDto>(message));
        //     // }

        //     // return BadRequest("Failed to send message");
        // }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessage([FromQuery] MessageParams messageParams)
        {
            messageParams.UserName = User.GetUserName();
            var messages = await _messageRepository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUserName = User.GetUserName();
            return Ok(await _messageRepository.GetMessageThread(currentUserName, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserName();
            var message = await _messageRepository.GetMessage(id);

            if (message.SenderUserName != username && message.RecipientUserName != username)
            {
                return Unauthorized();
            }

            if (message.SenderUserName == username)
            {
                message.SenderDeleted = true;
            }

            if (message.RecipientUserName == username)
            {
                message.RecipientDeleted = true;
            }

            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _messageRepository.DeleteMessage(message);
            }

            if (await _messageRepository.SaveAllAsync())
            {
                return Ok();
            }

            return BadRequest("Problem Deleting Message");
        }
    }
}