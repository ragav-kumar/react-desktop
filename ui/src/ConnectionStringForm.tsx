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
    );
};