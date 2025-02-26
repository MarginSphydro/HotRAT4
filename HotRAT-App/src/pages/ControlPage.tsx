import React, { useState, useEffect } from 'react';
import { getWork,addMessage,getSession } from '../api/ClientInfo';
import { ApiResponse, Session, ClientInfo ,Message} from '../api/ApiModels';
import { SessionID, updateToken } from '../runtimes';
import { Button, List, Card ,Input, Textarea} from '@telegram-apps/telegram-ui';
import { Loading } from '@/components/Loading.tsx';

const CustomListItem: React.FC<{
    history: Message;
    index: number;
}> = ({ history, index,}) => {
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const timer = setTimeout(() => {
            setLoading(false);
        }, 500);

        return () => clearTimeout(timer);
    }, []);

    if (loading) {
        return <Loading />;
    }

    return (
        <Card style={styles.infocard}>
            <div style={styles.row}>
                <div style={styles.index}>{index + 1}</div>
                <div style={styles.textContainer}>
                    <p><strong>消息码:</strong> {history.code}</p>
                    <p><strong>状态:</strong> {history.state}</p>
                    <p><strong>指令:</strong> {history.content}</p>
                    <p><strong>结果:</strong> {history.result}</p>
                </div>
            </div>
        </Card>
    );
};


const ControlPage: React.FC = () => {
  const [inputValue, setInputValue] = useState<string>('');
  const [histories, setHistories] = useState<Message[]>([]);

  useEffect(() => {
    const fetchHistories = async () => {
      const historiesData = (await getSession(updateToken(),SessionID)).data.historys;
      setHistories(historiesData);
    };

    fetchHistories();
  }, []);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setInputValue(e.target.value);
  };

  const handleButtonClick = () => {
    if (inputValue.trim() !== '') {
        addMessage(updateToken(),SessionID,inputValue);
      setInputValue('');
    }
  };

    return (
        <div>
            <div style={{ textAlign: 'center' }}>
                <h2>Control</h2>
                <span style={{ fontSize: 92 }}>🖥️</span>
            </div>
            <div style={{ textAlign: 'center',borderRadius:10}}>
                <Textarea type="text" value={inputValue} onChange={handleInputChange} />
                <Button style={{marginTop:10}} onClick={handleButtonClick}>Post Command</Button>
            </div>
            {histories.length === 0 ? (
                <Loading />
            ) : (
                <List>
                    {histories.map((history, index) => (
                        <CustomListItem
                            key={index}
                            index={index}
                            history={history}
                        />
                    ))}
                </List>
            )}
        </div>
    );
};

import { CSSProperties } from 'react';
const styles: { [key: string]: CSSProperties } = {
    infocard: {
        padding: '10px',
        borderRadius: '10px',
        boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
        backgroundColor: 'rgba(0,0,0,0.05)',
        width: '90%',
        margin: '10px 0 10px 10px'
    },
    row: {
        display: 'flex',
        alignItems: 'center',
    },
    index: {
        minWidth: '30px',
        textAlign: 'center',
        fontWeight: 'bold',
        marginRight: '10px',
    },
    textContainer: {
        flex: 1,
    },
    connectButton: {
        minWidth: '80px',
    },
};

export default ControlPage;