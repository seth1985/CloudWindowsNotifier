import React from 'react';

type Props = {
  title?: string;
  setTitle?: (v: string) => void;
  dynamicScript: string;
  setDynamicScript: (v: string) => void;
  dynamicMaxLength: string;
  setDynamicMaxLength: (v: string) => void;
  dynamicTrimWhitespace: boolean;
  setDynamicTrimWhitespace: (v: boolean) => void;
  dynamicFailIfEmpty: boolean;
  setDynamicFailIfEmpty: (v: boolean) => void;
  dynamicFallbackMessage: string;
  setDynamicFallbackMessage: (v: string) => void;
};

export const DynamicFields: React.FC<Props> = ({
  title: _title,
  setTitle: _setTitle,
  dynamicScript,
  setDynamicScript,
  dynamicMaxLength,
  setDynamicMaxLength,
  dynamicTrimWhitespace,
  setDynamicTrimWhitespace,
  dynamicFailIfEmpty,
  setDynamicFailIfEmpty,
  dynamicFallbackMessage,
  setDynamicFallbackMessage
}) => (
  <div className="form-card">
    <div className="section-head">
      <h4>Behavior</h4>
      <div className="button-row">
        <p className="hint">Runs a dynamic PowerShell script that returns title/message/link/icon.</p>
      </div>
    </div>
    <label>
      Dynamic script (PowerShell)
      <textarea
        placeholder="Write or paste the dynamic script..."
        value={dynamicScript}
        onChange={(e) => setDynamicScript(e.target.value)}
        className="script-area"
      />
    </label>
    <div className="stack two">
      <label>
        Max length
        <input
          type="number"
          min="1"
          value={dynamicMaxLength}
          onChange={(e) => setDynamicMaxLength(e.target.value)}
          className="narrow-input"
        />
      </label>
      <label className="checkbox-row left-align">
        <input
          type="checkbox"
          checked={dynamicTrimWhitespace}
          onChange={(e) => setDynamicTrimWhitespace(e.target.checked)}
        />
        <span>Trim whitespace</span>
      </label>
      <label className="checkbox-row left-align">
        <input
          type="checkbox"
          checked={dynamicFailIfEmpty}
          onChange={(e) => setDynamicFailIfEmpty(e.target.checked)}
        />
        <span>Fail if empty</span>
      </label>
      <label>
        Fallback message
        <input
          placeholder="Optional fallback message"
          value={dynamicFallbackMessage}
          onChange={(e) => setDynamicFallbackMessage(e.target.value)}
        />
      </label>
    </div>
  </div>
);
