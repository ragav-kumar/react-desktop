import './App.css'
import { ConnectionStringForm } from './ConnectionStringForm';
import { LogFeed } from './LogFeed';

export const App = () => (
    <div className='app'>
        <ConnectionStringForm />
        <LogFeed />
    </div>
);