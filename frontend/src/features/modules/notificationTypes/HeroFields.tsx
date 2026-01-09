import React from 'react';

type Props = {
  title: string;
  setTitle: (v: string) => void;
  message: string;
  setMessage: (v: string) => void;
};

export const HeroFields: React.FC<Props> = ({ title, setTitle, message, setMessage }) => (
  <div className="space-y-6">
    <div className="space-y-2.5">
      <span className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Main Hero Headline</span>
      <input
        className="input"
        placeholder="Enter prominent title"
        maxLength={60}
        value={title}
        onChange={(e) => setTitle(e.target.value.slice(0, 60))}
      />
    </div>
    <div className="space-y-2.5">
      <span className="block text-[10px] font-black text-text-secondary uppercase tracking-[0.15em] ml-1">Hero Description (Optional)</span>
      <textarea
        className="input h-32 resize-none"
        placeholder="Enter descriptive content for the hero banner..."
        maxLength={160}
        value={message}
        onChange={(e) => setMessage(e.target.value)}
      />
    </div>
  </div>
);
