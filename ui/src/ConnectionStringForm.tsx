import { Api } from './api';
import { type FormEvent, useEffect, useState } from 'react';

export const ConnectionStringForm = () => {
    const [connectionString, setConnectionString] = useState<string>('');

    const submit = async (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        await Api.SetConnectionString(connectionString);
        setConnectionString(await Api.GetConnectionString());
    };

    useEffect(() => {
        Api.GetConnectionString()
            .then(setConnectionString);
    }, []);

    return (
        <div className='connectionStringColumn'>
            <form className='connectionStringForm' onSubmit={submit}>
                <label>
                    Connection string
                </label>
                <input
                    value={connectionString}
                    onChange={(e) => setConnectionString(e.target.value)}
                />
                <button type='submit'>
                    Submit
                </button>
            </form>
            <div>
                <p>
                    The log file is updated everytime the connection string is written to or read from. The connection
                    string itself is stored in api state in the backend; this demo does not implement a config file.
                </p>
                <p>
                    The log feed on the right is initialized with the current state of the log file on mount.
                    It is also re-initialized when listening is turned on.
                </p>
                <p>
                    While listening, any writes to the log file are pushed to the UI and immediately appended.
                </p>
            </div>
        </div>
    );
};