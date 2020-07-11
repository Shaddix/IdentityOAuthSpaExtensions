const backendUri = `${window.location.protocol}//${window.location.hostname}:${window.location.port}`;
const getChallengeEndpoint = (provider) =>
    `${backendUri}/external-auth/challenge?provider=${provider}`;

function GetOAuthCode(provider) {
    function createOAuthMessageHandler(resolve, reject) {
        const handler = (event) => {
            if (event.data && event.data.type === 'oauth-result') {
                const data = event.data;
                if (data.code) {
                    resolve({
                        provider: data.provider,
                        code: data.code,
                    });
                } else {
                    reject({
                        provider: data.provider,
                        error: data.error,
                        errorDescription: data.errorDescription,
                    });
                }
                window.removeEventListener('message', handler, false);
            }
        };
        return handler;
    }

    return new Promise((resolve, reject) => {
        window.addEventListener(
            'message',
            createOAuthMessageHandler(resolve, reject),
            false,
        );
        window.open(
            getChallengeEndpoint(provider),
            undefined,
            'toolbar=no,menubar=no,directories=no,status=no,width=800,height=600',
        );
    });
}
