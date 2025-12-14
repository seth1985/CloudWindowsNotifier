import React from 'react';

type Props = {
  title: string;
  setTitle: (v: string) => void;
  message: string;
  setMessage: (v: string) => void;
};

export const StandardFields: React.FC<Props> = ({ title, setTitle, message, setMessage }) => (
  <>
    <label>
      Title
      <input
        placeholder="Notification title"
        maxLength={60}
        value={title}
        onChange={(e) => setTitle(e.target.value.slice(0, 60))}
      />
    </label>
    <label>
      Message
      <textarea
        placeholder="Notification message (160 chars)"
        maxLength={160}
        value={message}
        onChange={(e) => setMessage(e.target.value)}
      />
    </label>
  </>
);
