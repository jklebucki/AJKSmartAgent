import { useState } from 'react'
import type { FormEvent } from 'react'
import { useTranslation } from 'react-i18next'

export function ChatPanel() {
  const { t } = useTranslation()
  const [goal, setGoal] = useState('')

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
  }

  return (
    <section className="panel chat-panel" aria-labelledby="chat-title">
      <h2 id="chat-title">{t('chat.title')}</h2>
      <form onSubmit={handleSubmit}>
        <label className="sr-only" htmlFor="task-goal">
          {t('chat.placeholder')}
        </label>
        <textarea
          id="task-goal"
          value={goal}
          onChange={(event) => setGoal(event.target.value)}
          placeholder={t('chat.placeholder')}
          rows={5}
        />
        <button type="submit" disabled={!goal.trim()}>
          {t('chat.submit')}
        </button>
      </form>
      <p className="hint">{t('chat.hint')}</p>
    </section>
  )
}
